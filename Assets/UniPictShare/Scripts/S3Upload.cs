using UnityEngine;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using Amazon.CognitoIdentity;
using Amazon;
using System;
using UniRx;

namespace UniPictShare
{

    public class s3upload
    {

        private CognitoAWSCredentials credentials;
        private string backetNameString;
        private RegionEndpoint regionVal;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="IdentityPoolId">CognitoのIdentify Pool ID</param>
        /// <param name="backetName">バケット名</param>
        /// <param name="region">Region未指定の場合はUSEast1</param>
        public s3upload(string IdentityPoolId, string backetName, RegionEndpoint region = null)
        {
            AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
            if (region == null)
            {
                regionVal = RegionEndpoint.USEast1;
            } //Region未指定の場合はUSEast1を設定
            else
            {
                regionVal = region;
            }

            credentials = new CognitoAWSCredentials(IdentityPoolId, regionVal);
            backetNameString = backetName;
        }


        /// <summary>
        /// 指定ファイルをS3バケットにアップロードします
        /// </summary>
        /// <param name="inputFileFullPath">アップロードするローカルファイルパス</param>
        /// <param name="uploadS3path">S3パス。fol/filenameと指定するとfolフォルダ以下にアップロードする</param>
        /// <param name="observer">UniRxで完了通知する場合</param>
        public void uploadFileToS3(string inputFileFullPath, string uploadS3path, IObserver<Unit> observer = null)
        {
            AmazonS3Client S3Client = new AmazonS3Client(credentials, regionVal);

            //ファイル読み込み
            var stream = new FileStream(inputFileFullPath,
                FileMode.Open, FileAccess.Read, FileShare.Read);

            //リクエスト作成
            var request = new PostObjectRequest()
            {
                Bucket = backetNameString,
                Key = uploadS3path,
                InputStream = stream,
                CannedACL = S3CannedACL.PublicRead,
                Region = regionVal
            };

            //アップロード
            S3Client.PostObjectAsync(request, (responseObj) =>
            {
                if (responseObj.Exception == null)
                {
                    //Success
                    Debug.Log(uploadS3path + "   :Upload successed");
                    if (observer != null)
                    {
                        observer.OnNext(Unit.Default);
                        observer.OnCompleted();
                    }
                }
                else
                {
                    if(observer != null) observer.OnError(new Exception(responseObj.Response.HttpStatusCode.ToString()));
                    Debug.LogError(string.Format("\n receieved error {0}",
                        responseObj.Response.HttpStatusCode.ToString()));
                }
            });
        }

        /// <summary>
        /// データストリームをS3バケットにアップロードします
        /// </summary>
        /// <param name="stream">アップロードするデータのストリーム</param>
        /// <param name="uploadS3path">S3パス。fol/filenameと指定するとfolフォルダ以下にアップロードする</param>
        /// <param name="observer">UniRxで完了通知する場合</param>
        public void uploadStreamToS3(Stream stream, string uploadS3path, IObserver<Unit> observer = null)
        {
            AmazonS3Client S3Client = new AmazonS3Client(credentials, regionVal);

            //リクエスト作成
            var request = new PostObjectRequest()
            {
                Bucket = backetNameString,
                Key = uploadS3path,
                InputStream = stream,
                CannedACL = S3CannedACL.PublicRead,
                Region = regionVal
            };

            //アップロード
            S3Client.PostObjectAsync(request, (responseObj) =>
            {
                if (responseObj.Exception == null)
                {
                    //Success
                    Debug.Log(uploadS3path + "   :Upload successed");
                    if (observer != null)
                    {
                        observer.OnNext(Unit.Default);
                        observer.OnCompleted();
                    }
                }
                else
                {
                    if(observer != null) observer.OnError(new Exception(responseObj.Response.HttpStatusCode.ToString()));
                    Debug.LogError(string.Format("\n receieved error {0}",
                        responseObj.Response.HttpStatusCode.ToString()));
                }
            });
        }
    }
}