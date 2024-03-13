using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace AsyncEfCoreOdata.Controllers
{
  public class CustomersController : ODataController
  {
    /// <summary>
    /// Throw an error if the reader is not async
    /// </summary>
    private class ReaderInterceptor : DbCommandInterceptor
    {
      public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
      {
        throw new InvalidOperationException("Expect all calls to read to be async");
      }
    }

    [EnableQuery]
    public async Task<ActionResult> Get()
    {

      var options = new DbContextOptionsBuilder<MyDbContext>()
        .AddInterceptors(new ReaderInterceptor())
        .UseSqlServer(
          "Server=(localdb)\\mssqllocaldb;TrustServerCertificate=True;Trusted_Connection=True;MultipleActiveResultSets=true;pooling=true;Command Timeout=300;");
      await Task.Delay(100);
      // do not dispose so that lives for the whole request
      // this is a bad practice, but it is done here to demonstrate the issue
      var context = new MyDbContext(options.Options);

      // Use a Values statement to return customers
      return Ok(context.Database.SqlQueryRaw<Customer>(
        "Select 1 as Id, 'Customer 1' as Name union all Select 2 as Id, 'Customer 2' as Name union all Select 3 as Id, 'Customer 3' as Name")); 
    }

  }
  public class Customer
  {
    public int Id { get; set; }
    public string Name { get; set; }
  }
  public class MyDbContext : DbContext
  {
    public MyDbContext(DbContextOptions<MyDbContext> options)
      : base(options)
    {
      
    }
  }
}
