namespace OpenCvSharp.Demo
{
	using UnityEngine;
	using UnityEngine.UI;
	using OpenCvSharp;

	public class FaceDetectorScene : WebCamera
	{
        [SerializeField]
        private TextAsset faces;
        [SerializeField]
        private TextAsset eyes;
        [SerializeField]
        private TextAsset shapes;
        [SerializeField]
        private GameObject objects;
        [SerializeField]
        private Button rightB;
        [SerializeField]
        private Button leftB;
        [SerializeField]
        private Button upB;
        [SerializeField]
        private Button downB;
        [SerializeField]
        private Button xRatUp;
        [SerializeField]
        private Button xRatDown;
        [SerializeField]
        private Button yRatUp;
        [SerializeField]
        private Button yRatDown;
        [SerializeField]
        private Button scaleUp;
        [SerializeField]
        private Button scaleDown;
        [SerializeField]
        private Text positions;
        [SerializeField]
        public GameObject voiceCommand;

        public bool ispaused = false;
		private FaceProcessorLive<WebCamTexture> processor;
        private WebCamera cam;
        private float xPos = -9.4f;
        private float yPos = 34f;
        private float xRatio = 21f;
        private float yRatio = 12.6f;
        private float scaleshape = 0.33f;
        private float reset = 0;
        private float saveX = -5;
        private bool startSlideShow = false;
        private bool InternetConn;
        private bool photoTaken = false;
        private bool oneTimePaused = false;

        void Start()
        {
            rightB.onClick.AddListener(Right);
            leftB.onClick.AddListener(Left);
            upB.onClick.AddListener(Up);
            downB.onClick.AddListener(Down);
            xRatUp.onClick.AddListener(xRatioUp);
            xRatDown.onClick.AddListener(xRatioDown);
            yRatUp.onClick.AddListener(yRatioUp);
            yRatDown.onClick.AddListener(yRatioDown);
            scaleUp.onClick.AddListener(ScaleUp);
            scaleDown.onClick.AddListener(ScaleDown);

        }
		protected override void Awake()
		{
			base.Awake();
			base.forceFrontalCamera = true; // we work with frontal cams here, let's force it for macOS s MacBook doesn't state frontal cam correctly

			byte[] shapeDat = shapes.bytes;
			if (shapeDat.Length == 0)
			{
				string errorMessage =
					"In order to have Face Landmarks working you must download special pre-trained shape predictor " +
					"available for free via DLib library website and replace a placeholder file located at " +
					"\"OpenCV+Unity/Assets/Resources/shape_predictor_68_face_landmarks.bytes\"\n\n" +
					"Without shape predictor demo will only detect face rects.";

#if UNITY_EDITOR
				// query user to download the proper shape predictor
				if (UnityEditor.EditorUtility.DisplayDialog("Shape predictor data missing", errorMessage, "Download", "OK, process with face rects only"))
					Application.OpenURL("http://dlib.net/files/shape_predictor_68_face_landmarks.dat.bz2");
#else
             UnityEngine.Debug.Log(errorMessage);
#endif
			}
            cam = new FaceDetectorScene();
			processor = new FaceProcessorLive<WebCamTexture>();
			processor.Initialize(faces.text, eyes.text, shapes.bytes);

			// data stabilizer - affects face rects, face landmarks etc.
			processor.DataStabilizer.Enabled = true;        // enable stabilizer
			processor.DataStabilizer.Threshold = 2.0;       // threshold value in pixels
			processor.DataStabilizer.SamplesCount = 2;      // how many samples do we need to compute stable data

			// performance data - some tricks to make it work faster
			processor.Performance.Downscale = 256;          // processed image is pre-scaled down to N px by long side
			processor.Performance.SkipRate = 0;             // we actually process only each Nth frame (and every frame for skipRate = 0)
		}


        #region Tool to manually adjust the position of objects to fit mirror prespective not the camera one

        public void Right()
        {
            xPos = xPos + 0.2f;
        }

        public void Left()
        {
            xPos = xPos - 0.2f;
        }

        public void Up()
        {
            yPos = yPos + 0.2f;
        }

        public void Down()
        {
            yPos = yPos - 0.2f;
        }

        public void xRatioUp()
        {
            xRatio += 0.2f;
        }

        public void xRatioDown()
        {
            xRatio -= 0.2f;
        }

        public void yRatioUp()
        {
            yRatio += 0.2f;
        }

        public void yRatioDown()
        {
            yRatio -= 0.2f;
        }

        public void ScaleUp()
        {
            scaleshape += 0.03f;
        }

        public void ScaleDown()
        {
            scaleshape -= 0.03f;
        }
        #endregion


        private void ResetMouth()
        {
            photoTaken = false;
        }

		protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
		{
            if(Application.internetReachability == NetworkReachability.NotReachable)
            {
                InternetConn = false;
            }
            else
            {
                InternetConn = true;
            }

            positions.text = "X = " + xPos.ToString() + "  Y = " + yPos.ToString() + 
            "\nxRat = " + xRatio.ToString() + "  yRat = " + yRatio.ToString() + 
            "\nScaleShape = " + scaleshape.ToString();

			processor.ProcessTexture(input, TextureParameters);

            if (!voiceCommand.GetComponent<SpeechDetection>().showingPhotos)
            {
                try
                {
                    float diffforscale = Mathf.Abs(processor.converted[19].X/xRatio - processor.converted[24].X/xRatio);
                    float mouthOppened = Mathf.Abs(processor.converted[62].Y/yRatio - processor.converted[66].Y/yRatio);
                    float x = processor.converted[28].X/xRatio;
                    float y = processor.converted[28].Y/yRatio;
                    if (voiceCommand.GetComponent<SpeechDetection>().takingPhoto)
                    {
                        if (!oneTimePaused)
                        {
                            oneTimePaused = true;
                        }
                        objects.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f) * diffforscale;
                        objects.transform.position = new Vector3(processor.converted[28].X/25.2f, -processor.converted[28].Y/22.6f, 20f) + new Vector3(-8.8f - diffforscale/1.88f, 11.9f + diffforscale/10, 0f);
                    }
                    else
                    {
                        if (oneTimePaused)
                        {
                            ispaused = false;
                            oneTimePaused = false;
                        }
                        objects.transform.localScale = new Vector3(scaleshape, scaleshape, scaleshape) * diffforscale;
                        objects.transform.position = new Vector3(x, -y, 20f) + new Vector3(xPos - diffforscale/1.88f, yPos + diffforscale/10, 0f);
                    }

                    if (!InternetConn)
                    {
                        if (mouthOppened > 1.4f && photoTaken == false)
                        {
                            StartCoroutine(voiceCommand.GetComponent<SpeechDetection>().TakeAPicture()); 
                            photoTaken = true; 
                            Invoke("ResetMouth", 3f);
                        }
                    }

                    if (x > -1 && ispaused == false && x != saveX)
                    {
                        startSlideShow = false;
                        reset = 0;
                        ispaused = true;
                        reset = 0;
                        voiceCommand.GetComponent<SpeechDetection>().StartMirror();
                    }
                    else if (x != saveX)
                    {
                        saveX = x;
                        reset = 0;
                    }
                    else
                    {
                        reset += Time.fixedDeltaTime;
                        if (reset > 2f && ispaused == true)
                        {
                            voiceCommand.GetComponent<SpeechDetection>().Idle();
                            ispaused = false;
                        }
                        if (reset >= 6f && startSlideShow == false)
                        {
                            voiceCommand.GetComponent<SpeechDetection>().StartSlideShow();
                            startSlideShow = true;
                        }
                    }
                }
                catch{}
            }
			// mark detected objects
            if (!voiceCommand.GetComponent<SpeechDetection>().takingPhoto)
            {
			    processor.MarkDetected();
            }
			// processor.Image now holds data we'd like to visualize
			output = Unity.MatToTexture(processor.Image, output);   // if output is valid texture it's buffer will be re-used, otherwise it will be re-created
			return true;
		}
    }
}