using qm;

var builder = WebApplication.CreateBuilder(args);

var command = new QmCommand(builder);
var parse = command.Parse(args);
return await parse.InvokeAsync();
