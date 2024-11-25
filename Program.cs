using MonitorEventViewer.Components;
using MonitorEventViewer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
  .AddInteractiveServerComponents();

builder.Services.AddSingleton<EventLogAnalyzer>(sp => 
  new EventLogAnalyzer(".", "Application")); // Usando "." para máquina local

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error", createScopeForErrors: true);
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
  .AddInteractiveServerRenderMode();

app.Run();