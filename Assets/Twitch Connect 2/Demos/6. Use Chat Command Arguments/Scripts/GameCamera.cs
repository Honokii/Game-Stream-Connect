using UnityEngine;

namespace TwitchConnect.Demo.UseChatCommandArguments
{
    [System.Serializable]
    public class GameCamera
    {

#region Variables
        /// <summary>
        /// define the Camera (from the scene) in inspector
        /// </summary>
        [SerializeField]
        private Camera camera;

        /// <summary>
        /// what the argument should be for this camera.
        /// Whitespaces will be removed from this (command arguments are one word only).
        /// </summary>
        [SerializeField]
        private string argument = "";

        /// <summary>
        /// Should this camera be enabled on start
        /// </summary>
        [SerializeField]
        private bool enableOnStart;

        /// <summary>
        /// internal field to know if the above information is valid
        /// </summary>
        private bool valid;
        #endregion

#region Public methods
        /// <summary>
        /// this will be called on start by cemra manager
        /// </summary>
        public void InitializeCamera()
        {
            valid = CheckParametersValid();

            // enable camera on start if needed
            if (enableOnStart)
                EnableCamera();
            else
                DisableCamera();
        }

        /// <summary>
        /// expose a public method to enable/disable this camera from a boolean value.
        /// if no argument is passed, enable the camera (only if it is a valid camera)
        /// </summary>
        /// <param name="active">Enable or disable the camera. Default = true</param>
        public void SetCameraActive(bool active = true)
        {
            if (valid && active)
                EnableCamera();
            else
                DisableCamera();
        }

        /// <summary>
        /// expose a public method to enable/disable this camera from an argument value.
        /// will enable if the passed argument matches the argument of this camera
        /// </summary>
        /// <param name="_argument">If this parameter equals this camera argument, will enable. Will disable otherwise</param>
        public void SetCameraActive(string _argument)
        {
            if (valid && _argument == argument)
                EnableCamera();
            else
                DisableCamera();
        }
#endregion

#region Private methods

        /// <summary>
        /// What to do when enabling camera.
        /// You can add stuff about audiolisteners etc. here.
        /// </summary>
        private void EnableCamera()
        {
            camera.gameObject.SetActive(true);
        }

        /// <summary>
        /// What to do when disabling camera.
        /// You can add stuff about audiolisteners etc. here.
        /// </summary>
        private void DisableCamera()
        {
            camera.gameObject.SetActive(false);
        }

        /// <summary>
        /// Check if the camera is properly defined.
        /// </summary>
        /// <returns>Are parameters valid or not. Of not valid, the camera will not be enabled no matter what</returns>
        private bool CheckParametersValid()
        {
            if (camera == null)
            {
                Debug.LogWarning("[GameCamera] A Camera reference is null or missing.");
                return false;
            }


            if (argument == null || argument.Length == 0)
            {
                Debug.LogWarning("[GameCamera] A Camera argument is null or empty.");
                return false;
            }


            if (argument.Contains(" "))
            {
                argument.Trim();
                Debug.LogWarning("[GameCamera] A Camera argument contains a whitespace that has been removed. The argument used is now : " + argument);
            }

            return true;
        }
#endregion

    }
}