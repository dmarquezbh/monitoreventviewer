using MonitorEventViewer.Components;
using MonitorEventViewer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
  .AddInteractiveServerComponents();

// builder.Services.AddSingleton<EventLogAnalyzer>(sp => 
//   new EventLogAnalyzer(".", "Application")); // Usando "." para máquina local
// builder.Services.AddScoped<EventLogAnalyzer>();
// builder.Services.AddScoped(sp => new EventLogAnalyzer("localhost", "Application"));
builder.Services.AddScoped<EventLogAnalyzer>(sp => new EventLogAnalyzer("localhost", "Application"));


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error", createScopeForErrors: true);
  app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
  .AddInteractiveServerRenderMode();

app.Run();