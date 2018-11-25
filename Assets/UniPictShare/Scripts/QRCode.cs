using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using ZXing.QrCode;

namespace UniPictShare
{
    // Inspired from negipoyoc https://negipoyoc.com/blog/making-qrcode-with-unity/
    public class QRCodeUtil
    {
        /// <summary>
        /// QRコード読み取り)(返り文字列が-1の場合は、読み込めていない)
        /// </summary>
        /// <param name="cameraTexture">Camera texture.</param>
        public Result Read(WebCamTexture cameraTexture)
        {
            var reader = new BarcodeReader();
            var color = cameraTexture.GetPixels32();
            var width = cameraTexture.width;
            var height = cameraTexture.height;
            var result = reader.Decode(color, width, height);

            return result;
        }


        /// <summary>
        /// QRコード作成
        /// </summary>
        /// <param name="inputString">QRコード生成元の文字列</param>
        /// <param name="textture">QRの画像がここに入る</param>
        public Texture2D Create(string inputString, int width, int height)
        {
            var texture = new Texture2D(width, height);
            var qrCodeColors = Write(inputString, width, height);
            texture.SetPixels32(qrCodeColors);
            texture.Apply();

            return texture;
        }


        private Color32[] Write(string content, int width, int height)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = height,
                    Width = width
                }
            };

            return writer.Write(content);
        }
    }
}
