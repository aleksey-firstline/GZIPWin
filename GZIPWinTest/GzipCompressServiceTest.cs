using System;
using System.Text;
using GZIPWin.Services;
using NUnit.Framework;

namespace GZIPWinTest
{
    public class GzipCompressServiceTest
    {
        [Test]
        public void CompressedBytes_ShouldBeEqual()
        {
			//Test43534
            var compressedBytes = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 0, 11, 61, 78, 219, 13, 131, 48, 12, 92, 229, 6, 168, 58, 73, 151, 112, 140, 17, 150, 18, 135, 250, 193, 252, 5, 209, 246, 235, 164, 123, 191, 166, 203, 128, 238, 81, 3, 196, 92, 35, 200, 192, 229, 36, 160, 174, 239, 162, 191, 232, 92, 88, 165, 107, 192, 202, 24, 187, 79, 181, 159, 231, 1, 155, 145, 78, 208, 5, 174, 81, 113, 85, 92, 176, 81, 211, 164, 8, 129, 248, 252, 179, 60, 173, 154, 210, 157, 150, 196, 74, 197, 218, 78, 129, 22, 172, 21, 44, 56, 102, 175, 220, 41, 97, 218, 54, 176, 83, 224, 56, 183, 19, 100, 249, 125, 118, 230, 82, 122, 191, 235, 22, 29, 98, 89, 227, 249, 1, 245, 83, 144, 214, 208, 0, 0, 0 };
            var bytes = Encoding.ASCII.GetBytes("Lorem ipsum accumsan curae aliquam ipsum arcu felis nunc proin aliquam, nostra id risus cursus habitasse eros cursus conubia aliquet faucibus ad fusce volutpat nibh cras velit ante aliquet tellus condimentum.");
            var compressService = new GzipCompressService();
            var processedBytes = compressService.Process(bytes);

            Assert.IsNotNull(processedBytes);
            Assert.AreEqual(processedBytes, compressedBytes);
        }
    }
}