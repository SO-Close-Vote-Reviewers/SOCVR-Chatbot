using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace TheCommonLibrary.FTP
{
    public class FTPManager
    {
        private Uri _siteUri;
        private string _username;
        private string _password;

        public FTPManager(string siteUrl, string username, string password)
        {
            _siteUri = new Uri(siteUrl);
            _username = username;
            _password = password;
        }

        public void UploadFile(string filePath, string uploadTo)
        {
            string fullServerUrl = new Uri(_siteUri, uploadTo).ToString();

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(fullServerUrl);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            request.Credentials = new NetworkCredential(_username, _password);

            byte[] fileContents;
            using (StreamReader sourceStream = new StreamReader(filePath))
            {
                fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                // sourceStream gets closed by the Dispose call caused by the using
            }

            request.ContentLength = fileContents.Length;

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            response.Close();
        }

        
    }
}
