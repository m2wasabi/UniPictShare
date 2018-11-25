using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Amazon;
using UniRx;
using UnityEngine.UI;

namespace UniPictShare
{
    // Inspired from negipoyoc https://gist.github.com/negipoyoc/5bf5db8e0187c167a823cb05ff1b2182
    public class ScreenShot : MonoBehaviour
    {
//        private SoundEffect _soundEffect;
        [SerializeField]
        private string IdentityPoolId;

        // ここだけ決め打ち…
        private RegionEndpoint regionVal = RegionEndpoint.APNortheast1;

        [SerializeField]
        private string backetName;

        private s3upload s3;

        [SerializeField]
        private GameObject UICanvas;

        [SerializeField]
        private GameObject QRPanel;

        [SerializeField]
        private RawImage QRCode;
        
        void Start()
        {
            // Prepare AWS S3
            UnityInitializer.AttachToGameObject(this.gameObject);

            s3 = new s3upload(IdentityPoolId, backetName, regionVal);
        }

        public void TakeScreenShot()
        {
            if (UICanvas != null) UICanvas.SetActive(false);
            Observable
                .NextFrame()
                .Subscribe(_ => StartCoroutine(CaptureWithAlpha()));
            ;
        }

        public void HideQECode()
        {
            if (QRPanel.activeSelf)
            {
                QRPanel.SetActive(false);
            }
        }

        IEnumerator CaptureWithAlpha()
        {
            yield return new WaitForEndOfFrame();

            var tex = ScreenCapture.CaptureScreenshotAsTexture();

            var width = tex.width;
            var height = tex.height;
            var texAlpha = new Texture2D(width, height, TextureFormat.ARGB32, false);
            // Read screen contents into the texture
            texAlpha.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texAlpha.Apply();
            if (UICanvas != null) UICanvas.SetActive(true);

            // Encode texture into PNG
            var bytes = texAlpha.EncodeToPNG();
            Destroy(tex);
            
            // store to file
//            File.WriteAllBytes(Application.streamingAssetsPath + "/" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png", bytes);

            // store to S3
            var stream = new MemoryStream(bytes);
            var uploadS3path = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_" + Guid.NewGuid().ToString ("N").Substring(0, 10) + ".png";
            Subject<Unit> uploadSubject = new Subject<Unit>();
            s3.uploadStreamToS3(stream,uploadS3path,uploadSubject);
            uploadSubject.Subscribe(_ =>
            {
                var qrUtil = new QRCodeUtil();
                var qrTexture = qrUtil.Create("https://s3-" + regionVal.SystemName + ".amazonaws.com/" + backetName + "/" + uploadS3path, 256 , 256);
                QRCode.texture = qrTexture;
                QRPanel.SetActive(true);
            });
        }

        
    }
}