using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Default
{
    public class PlayerController : MonoBehaviour
    {
        [HideInInspector] public Transform focusedObject;
        [SerializeField] private float mouseSensitivityX = 2f;
        [SerializeField] private float mouseSensitivityY = 2f;

        [Space(10), SerializeField] private bool tankControl = false;
        [SerializeField] private float tankControlRotationSpeed = 1.2f;
        [Space(10), SerializeField] private float speedNormal = 5f;
        [SerializeField] private float speedSneaking = 2f;
        [SerializeField] private float speedSprinting = 8f;
        [SerializeField] private float jumpForce = 1f;
        public float speedCurrent { get; private set; }
        [SerializeField] private float gravity = 10f;
        [SerializeField] private float slopeLimit = 80f;
        [SerializeField] private float slideSpeed = 6f;
        private Vector3 moveDirection = Vector3.zero;

        [Space(10), SerializeField] private float heightNormal = 1.8f;
        [SerializeField] private float heightSneaking = 1.4f;
        [SerializeField] private FOVController fOVController;
        [SerializeField] private Vector2 xRotationClamp = new Vector2(-50, 90);

        [Space(10)] private int frozenSem = 0;
        private Vector3 hitNormal;
        private bool isSliding = false;
        private bool isSneaking = false;
        private bool isSprinting = false;

        [Space(10), SerializeField] private GameObject crosshair;
        [SerializeField] private Animator locomotionAnimator;

        [Space(10), SerializeField] private CharacterController charController;
        [SerializeField] private Transform eyeHeightTransform;
        [SerializeField] private UseController useController;

        void Start()
        {
            speedCurrent = speedNormal;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1;
            SetFrozen(false);
        }

        void Update()
        {
            //Do nothing when Player isn't allowed to move
            if (IsFrozen())
                return;

            //Get Inputs
            MoveData inputs = new MoveData()
            {
                xRot = Input.GetAxis("Mouse Y") * mouseSensitivityY,
                yRot = Input.GetAxis("Mouse X") * mouseSensitivityX,
                axisHorizontal = Input.GetAxisRaw("Horizontal"),
                axisVertical = Input.GetAxisRaw("Vertical"),
                axisSneak = Input.GetAxisRaw("Control"),
                axisSprint = Input.GetAxisRaw("Shift"),
                axisJump = Input.GetAxisRaw("Space"),
                axisPrimary = Input.GetAxisRaw("Primary"),
                axisSecondary = Input.GetAxisRaw("Secondary")
            };

            if (tankControl)
            {
                Rotate(0f, inputs.axisHorizontal * tankControlRotationSpeed * (inputs.axisSprint + 1f) * Time.deltaTime);
                inputs.xRot = 0;
                inputs.yRot = 0;
                inputs.axisHorizontal = 0;
            }
            else if (focusedObject == null)
                Rotate(inputs.xRot, inputs.yRot);

            if (focusedObject != null)
                FocusObject();

            Move(inputs);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            hitNormal = hit.normal;
            isSliding = Vector3.Angle(Vector3.up, hitNormal) >= slopeLimit;
        }

        void Rotate(float xRot, float yRot)
        {
            Quaternion characterTargetRot = transform.localRotation;
            Quaternion cameraTargetRot = eyeHeightTransform.localRotation;

            characterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            cameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            cameraTargetRot = ClampRotationAroundXAxis(cameraTargetRot);

            transform.localRotation = characterTargetRot;
            eyeHeightTransform.localRotation = cameraTargetRot;
        }

        void FocusObject()
        {
            Quaternion oldRot = Quaternion.Euler(eyeHeightTransform.localEulerAngles.x, transform.localEulerAngles.y, 0f);
            Quaternion newRot = Quaternion.LookRotation(focusedObject.position - eyeHeightTransform.position);

            Quaternion targetRot = Quaternion.Lerp(oldRot, newRot, 8f * Time.deltaTime);

            transform.localEulerAngles = new Vector3(0, targetRot.eulerAngles.y, 0);
            eyeHeightTransform.localEulerAngles = new Vector3(targetRot.eulerAngles.x, 0, 0);
        }

        void Move(MoveData inputs)
        {
            //Changes camera height and speed while sneaking
            if (inputs.axisSneak > 0)
            {
                if (!isSneaking)
                    StartCoroutine(Sneak(true));

                if (isSprinting)
                    Sprint(false);
            }
            else
            {
                if (isSneaking)
                    StartCoroutine(Sneak(false));

                if (!isSneaking)
                {
                    //Changes camera FOV and speed while sprinting
                    if (inputs.axisSprint > 0 && inputs.axisVertical > 0)
                    {
                        if (!isSprinting)
                            Sprint(true);
                    }
                    else
                    {
                        if (isSprinting)
                            Sprint(false);
                    }
                }
            }
            speedCurrent = isSneaking ? speedSneaking : isSprinting ? speedSprinting : speedNormal;

            //Normalize input and add speed
            Vector2 input = new Vector2(inputs.axisHorizontal, inputs.axisVertical);
            input.Normalize();
            input *= speedCurrent;

            //Jump and Gravity
            if (tankControl)
                moveDirection = transform.forward * input.y + transform.up * moveDirection.y;
            else
                moveDirection = transform.forward * input.y + transform.right * input.x + transform.up * moveDirection.y;

            if (charController.isGrounded)
            {
                if (isSliding)
                {
                    //Slide
                    float slideX = ((1f - hitNormal.y) * hitNormal.x) * slideSpeed;
                    float slideZ = ((1f - hitNormal.y) * hitNormal.z) * slideSpeed;

                    if (Mathf.Sign(moveDirection.x) != Mathf.Sign(slideX))
                        moveDirection.x = 0;
                    if (Mathf.Sign(moveDirection.z) != Mathf.Sign(slideZ))
                        moveDirection.z = 0;

                    moveDirection.x += slideX;
                    moveDirection.z += slideZ;
                }
                else
                {
                    if (inputs.axisJump > 0)
                        moveDirection.y = jumpForce;
                    else
                        moveDirection.y = -gravity;
                }
            }
            moveDirection.y -= gravity * Time.deltaTime;

            //Move and animate
            Vector3 oldPos = transform.localPosition;
            charController.Move(moveDirection * Time.deltaTime);
            Vector3 deltaPos = oldPos - transform.localPosition;

            bool isMoving = (inputs.axisHorizontal > 0.1f || inputs.axisVertical > 0.1f || inputs.axisHorizontal < -0.1f || inputs.axisVertical < -0.1f) && !(deltaPos.y > 0.1f || deltaPos.y < -0.1f) && deltaPos.sqrMagnitude > 0f;
            locomotionAnimator.SetBool("walking", isMoving);
            locomotionAnimator.SetBool("running", isMoving && isSprinting);
            locomotionAnimator.SetFloat("forward", input.y);
            locomotionAnimator.SetFloat("right", input.x);
        }

        IEnumerator Sneak(bool willSneak)
        {
            isSneaking = willSneak;
            charController.height = willSneak ? heightSneaking : heightNormal;
            charController.center = Vector3.up * (charController.height / 2f);

            Vector3 oldCamPos = eyeHeightTransform.localPosition;
            Vector3 newCamPos = new Vector3(0f, willSneak ? heightSneaking : heightNormal, 0f);

            for (float i = 0; i < 1 && (isSneaking == willSneak); i += 0.2f)
            {
                eyeHeightTransform.localPosition = Vector3.Lerp(oldCamPos, newCamPos, i);
                yield return new WaitForSeconds(1f / 60f);
            }
            if (isSneaking == willSneak)
                eyeHeightTransform.localPosition = newCamPos;
        }

        private void Sprint(bool willSprint)
        {
            isSprinting = willSprint;
            fOVController.isSprinting = willSprint;
        }

        public bool IsFrozen() { return frozenSem <= 0; }

        public void SetFrozen(bool frozen)
        {
            frozenSem += frozen ? -1 : 1;

            if (frozenSem > 1)
            {
                frozenSem = 1;
                Debug.LogWarning(gameObject.name + " got unfrozen twice!");
            }
            else if (frozenSem == 1)
            {
                crosshair.SetActive(true);
                useController.enabled = true;
            }
            else if (frozenSem == 0)
            {
                crosshair.SetActive(false);
                useController.enabled = false;

                Sprint(false);
                StartCoroutine(Sneak(false));

                locomotionAnimator.SetBool("walking", false);
                locomotionAnimator.SetBool("running", false);
                locomotionAnimator.SetFloat("forward", 0);
                locomotionAnimator.SetFloat("right", 0);
            }
        }

        public void TeleportPlayer(Vector3 positionNew, Vector3 newRotation)
        {
            bool oldCCState = charController.enabled;
            charController.enabled = false;

            transform.position = positionNew;
            transform.rotation = Quaternion.Euler(0f, newRotation.y, 0f);
            eyeHeightTransform.localRotation = Quaternion.Euler(newRotation.x, 0f, 0f);

            charController.enabled = oldCCState;
        }

        public void TeleportPlayer(Transform newPosition)
        {
            TeleportPlayer(newPosition.position, newPosition.eulerAngles);
        }

        public void TeleportAndRotateAround(Vector3 newPosition, Vector3 rotatePoint, float rotateAngle)
        {
            bool oldCCState = charController.enabled;
            charController.enabled = false;

            transform.position = newPosition;
            transform.RotateAround(rotatePoint, Vector3.up, rotateAngle);

            charController.enabled = oldCCState;
        }

        public IEnumerator MoveRotatePlayer(Transform newPosition, float seconds = 2f)
        {
            if (isSprinting)
                Sprint(false);

            Vector3 positionNew = newPosition.position;

            var mov = StartCoroutine(MovePlayer(positionNew, seconds));
            var rot = StartCoroutine(RotatePlayer(newPosition.rotation, seconds));

            yield return mov;
            yield return rot;
        }

        public IEnumerator MovePlayer(Vector3 newPosition, float seconds = 2f, bool ignoreSneak = false)
        {
            if (isSprinting)
                Sprint(false);
            if (isSneaking && !ignoreSneak)
                StartCoroutine(Sneak(false));

            Vector3 positionOld = transform.position;

            float rate = 1f / seconds;
            float fSmooth;
            for (float f = 0f; f <= 1f; f += rate * Time.deltaTime)
            {
                fSmooth = Mathf.SmoothStep(0f, 1f, f);
                transform.position = Vector3.Lerp(positionOld, newPosition, fSmooth);

                yield return null;
            }

            transform.position = newPosition;
        }

        public IEnumerator RotatePlayer(Quaternion newRotation, float seconds = 2f)
        {
            if (isSprinting)
                Sprint(false);

            Quaternion rotationPlayerOld = transform.rotation;
            Quaternion rotationCameraOld = eyeHeightTransform.localRotation;

            Quaternion rotationPlayerNew = Quaternion.Euler(0f, newRotation.eulerAngles.y, 0f);
            Quaternion rotationCameraNew = Quaternion.Euler(newRotation.eulerAngles.x, 0f, 0f);

            float rate = 1f / seconds;
            float fSmooth;
            for (float f = 0f; f <= 1f; f += rate * Time.deltaTime)
            {
                fSmooth = Mathf.SmoothStep(0f, 1f, f);
                eyeHeightTransform.localRotation = Quaternion.Lerp(rotationCameraOld, rotationCameraNew, fSmooth);
                transform.rotation = Quaternion.Lerp(rotationPlayerOld, rotationPlayerNew, fSmooth);

                yield return null;
            }

            eyeHeightTransform.localRotation = rotationCameraNew;
            transform.rotation = rotationPlayerNew;
        }

        public IEnumerator LookAt(Vector3 lookAtPos, float seconds = 2f)
        {
            yield return RotatePlayer(Quaternion.LookRotation(lookAtPos - eyeHeightTransform.position), seconds);
        }

        public void SetRotationLerp(Vector3 a, Vector3 b, float t)
        {
            Quaternion a1 = Quaternion.Euler(0f, a.y, 0f);
            Quaternion a2 = Quaternion.Euler(a.x, 0f, 0f);
            Quaternion b1 = Quaternion.Euler(0f, b.y, 0f);
            Quaternion b2 = Quaternion.Euler(b.x, 0f, 0f);

            transform.rotation = Quaternion.Lerp(a1, b1, t);
            eyeHeightTransform.localRotation = Quaternion.Lerp(a2, b2, t);
        }

        public Vector3 GetRotation()
        {
            return new Vector3(eyeHeightTransform.localEulerAngles.x, transform.eulerAngles.y, 0f);
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angleX = Mathf.Clamp(angleX, xRotationClamp.x, xRotationClamp.y);
            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }
    }
}
