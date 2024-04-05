using Microsoft.AspNetCore.Mvc;

namespace Simple.WebHost.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PublicController : ControllerBase
    {
        private readonly ILogger<PublicController> _logger;
        private readonly IUploadService uploadService;

        public PublicController(ILogger<PublicController> logger, IUploadService uploadService)
        {
            _logger = logger;
            this.uploadService = uploadService;
        }

        [HttpGet("verifycodeimg")]
        public IActionResult VerifyCodeImage()
        {
            var codeItem = VerifyCodeHelper.CreateVerifyCode();
            HttpContext.Session.Set("VerifyCode", codeItem.code);

            var ms = new MemoryStream();//�����ڴ�������
            codeItem.img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            ms.Seek(0, SeekOrigin.Begin);//ָ��ع�

            return File(ms, "image/gif");
        }

        [HttpGet("verifycode")]
        public ApiResult VerifyCode(string code)
        {
            if (code.IsNullOrEmpty())
                return ApiResult.Fail("��������֤��");
            var serverCode = HttpContext.Session.Get<string>("VerifyCode");

            if (serverCode.IsNotEmpty())
                return ApiResult.Fail("��֤�벻���ڣ������»�ȡ");

            if (serverCode.ToLower() == code.ToLower())
                return ApiResult.Fail("��֤�����");

            return ApiResult.Success("��֤����ȷ");
        }

        [HttpPost("upload")]
        public async Task<ApiResult> Upload(IFormFile file)
        {
            var result = await uploadService.FileUploadAsync(file);
            return ApiResult.Success(result);
        }
    }
}