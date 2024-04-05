using Microsoft.OpenApi.Models;
using Simple.WebHost;

using Swashbuckle.AspNetCore.Filters;

var start = DateTime.Now;
ConsoleHelper.Debug($"��ʼ��������");
var builder = WebApplication.CreateBuilder(args);
if (builder.Configuration["ConfigEnv"].ToString().ToUpper() == "DEV")
{
    ConsoleHelper.Debug($"��⵽��ǰ�ǿ���������ʹ�������ļ���Config/app_dev.json");
    builder.Configuration.AddJsonFile("Config/app_dev.json", true, true);
}
else
{
    ConsoleHelper.Debug($"��⵽��ǰΪ��ʽ������ʹ�������ļ���Config/app.json");
    builder.Configuration.AddJsonFile("Config/app.json", true, true);
}

#region ��ӷ�������

ConfigHelper.Init(builder.Configuration);

builder.Services.AddControllerConfig();

//����Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "API�ӿ�", Version = "v1" });
    foreach (var item in HostServiceExtension.XmlCommentsFilePath)
    {
        option.IncludeXmlComments(item, true);
    }
    //option.OperationFilter<SwaggerAuthOperatFilter>();
    //��api���token����֤��
    option.OperationFilter<SecurityRequirementsOperationFilter>();
    //option.DocumentFilter<HiddenApiFilter>(); // �ڽӿ��ࡢ����������� [HiddenApi]��������ֹ��Swagger�ĵ�������
    option.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "JWT��Ȩ(���ݽ�������ͷ�н��д���) ֱ�����¿�������Bearer {token}��ע������֮����һ���ո�\"",
        Name = ConfigHelper.GetValue("TokenHeadKey"),//jwtĬ�ϵĲ�������
        In = ParameterLocation.Header,//jwtĬ�ϴ��Authorization��Ϣ��λ��(����ͷ��)
        Type = SecuritySchemeType.ApiKey
    });
});
builder.Services.AddRedisCacheAndSession();
builder.Services.AddJwtAuth();
builder.Services.AddCustomerCors();
HostServiceExtension.BuildHostService(builder.Services, builder.Configuration);
ConsoleHelper.Debug($"��������������ɣ���ʼ���ùܵ�");

#endregion ��ӷ�������

//���������Ĺܵ�����
var app = builder.Build();
app.UseSession();
HostServiceExtension.BuildHostApp(app);
app.AddJobScheduler();
//����Swagger ShowSwagger = true ʱ��ʾswagger
if (ConfigHelper.GetValue<bool>("ShowSwagger"))
{
    ConsoleHelper.Waring($"Waring������������Swagger���б�¶API����");
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCustomerCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
ConsoleHelper.Debug($"���������ܵ���ɣ�׼������");
app.Run();