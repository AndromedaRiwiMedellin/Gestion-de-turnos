using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShiftManagement.Data;
using ShiftManagement.Models;

namespace ShiftManagement.Services;

public class PrintBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public PrintBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<MysqlDbContext>();
                
                var turnToPrint = await context.Turns
                    .Where(t => !t.IsPrinted)
                    .OrderBy(t => t.CreatedAt)
                    .FirstOrDefaultAsync(stoppingToken);

                if (turnToPrint != null)
                {
                    SendToCups(turnToPrint.Code);
                    
                    turnToPrint.IsPrinted = true;
                    await context.SaveChangesAsync(stoppingToken);
                }
            }
            await Task.Delay(3000, stoppingToken);
        }
    }

    private void SendToCups(string code)
    {
        try 
        {
            string ticket = $"   SHIFT SYSTEM\n" +
                            $"---------------------\n" +
                            $"    TICKET: {code}\n" +
                            $"---------------------\n" +
                            $" {DateTime.Now:MM/dd/yyyy HH:mm}\n\n\n\n";

            // Comando lp para la impresora configurada en CUPS
            string command = $"echo -e '{ticket}' | lp -d ImpresoraTickets";

            Process.Start("/bin/bash", $"-c \"{command}\"");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Printer Error: {ex.Message}");
        }
    }
}