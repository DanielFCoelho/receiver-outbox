using DepositOrderCreationCAP.Database;
using DotNetCore.CAP.Messages;
using Microsoft.EntityFrameworkCore;
using ReceiverOutbox.Receivers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DOCreationContext>(opt => opt.UseSqlServer("Server=tcp:sql-payin-payout-dev.database.windows.net,1433;Initial Catalog=sqldb-payin-payout-dev;Persist Security Info=False;User ID=CloudSA92a55aeb;Password=A1B2C3@!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"));

builder.Services.AddCap(opt =>
{
    //opt.DefaultGroupName = ""; O nome padr�o do grupo de consumidores corresponde a diferentes nomes em diferentes transportes. Voc� pode personalizar esse valor para personalizar os nomes nos transportes para facilitar a visualiza��o.
    //opt.GroupNamePrefix = ""; //Adiciona prefixos unificados para o grupo de consumidores.
    //opt.TopicNamePrefix = ""; //Adiciona prefixos unificados para o t�pico/fila de consumidores.
    opt.Version = "v1";
    opt.UseStorageLock = false; // Se for true, uma nova tabela � criara para resolver problemas de concorr�ncia.
    opt.CollectorCleaningInterval = 300; // intervalo em que as mensagens expiradas s�o deletadas.
    opt.ConsumerThreadCount = 1; //N�mero de threads simultaneas consumidas. Se for maior que 1, a order de execu��o n�o � garantida.
    opt.FailedRetryCount = 10; // N�mero de retries
    opt.FailedThresholdCallback = (FailedInfo failedInfo) => { Console.WriteLine($"Erro no callback: {failedInfo.Message}"); }; // A��o executada depois que a quantidade de retries � atingida
    opt.SucceedMessageExpiredAfter = (365 * 24 * 3600); //Tempo de expira��o de mensagem com sucesso.
    opt.FailedMessageExpiredAfter = (365 * 24 * 3600); //Tempo de expira��o de mensagem com erro.

    opt.UseEntityFramework<DOCreationContext>(k =>
    {
        k.Schema = "dbo";
    });
    opt.UseAzureServiceBus(k =>
    {
        k.ConnectionString = "Endpoint=sb://sb-caas-core-dev.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=sAJQ526PW5TvUa2c9dFPxYw8Qf1U5W/Pf+ASbEJBJw8=";
        k.TopicPath = "sbt-cap-events-test";
    });
    //opt.UseDashboard(k => k.PathMatch = "/myCap");
    //opt.UseDiscovery(k =>
    //{
    //    k.DiscoveryServerHostName = "localhost";
    //    k.DiscoveryServerPort = 8500;
    //    k.CurrentNodeHostName = "localhost";
    //    k.CurrentNodePort = 5239;
    //    k.NodeId = "1";
    //    k.NodeName = "CAP No.1 Node";
    //});
});

builder.Services.AddTransient<Receiver>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.Run();

