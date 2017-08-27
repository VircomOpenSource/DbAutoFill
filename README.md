# DbAutoFill 
DbAutoFill is a lightweight library mapping your objects to work with your database.
Unlike most ORM, DbAutoFill **does not** create a mapping to your database's schema.
Instead, it allows you map objects to results and commands, which is closer to what 
a developer will really deal with in the end.

## Example (How Does This Work)
Here's an example, where the `Article` object is naturally mapped to the stored procedure `myschema.CreateArticle`. 
The goal achieved here is to automatically fill the stored procedure variables with corresponding values from `Article` object. 
The mapping is done on fields name.
```
[DbAutoFill(ParameterPrefix = "@p_")]
public class Article
{
    [DbAutoFill(FillBehavior = FillBehavior.FromDB)]
    public int Id { get; set; }
    
    public string Title { get; set; }
    public string Author { get; set; }
    public string Content { get; set; }
    
    [DbAutoFill(DbType = DbType.DateTime2)]
    public DateTime PublishDate { get; set; }
}
```

```
CREATE PROCEDURE myschema.CreateArticle   
    @p_Title NVARCHAR(255),   
    @p_Author NVARCHAR(100),
    @p_PublishDate DATETIME2,
    @p_Content TEXT    
AS   
    -- ... do stuff
```

```
// ...
public DbResponse<MyResultType> CreateArticle(Article article)
{
    // With the Stored Procedure help:
    // throw on invalid connection string
    SqlCommandHelper helper = new SqlCommandHelper("myConnectionString...", "myschema");
    
    // Will call "myschema.CreateArticle" stored procedure.
    return helper.ExecuteDbProcedureNamedAsCallerName<MyResultType>(article);
    
    // Alternatively, do it manually, allowing to make unions of objects, 
    // or using manual queries instead of stored procedures.
    // Instead of having 4 command.AddParameterWithValue, you'll have this:
    //...
    using(SqlCommand command = new SqlCommand(connection))
    {
        command.Text = "myschema.CreateArticle";
        command.CommandType = CommandType.StoredProcedure;
        DbAutoFillHelper.AddParametersFromObjectMembers(command, article); // saves 4 lines of AddParameterWithValue.
        SqlDataReader dr = command.ExecuteReader();
        MyResultType result = new MyResultType();
        DbAutoFillHelper.FillObjectFromDataReader(result, dr);
    }
    // Notice the later doesn't catch exceptions, while the first method guarantees to catch exceptions
    // and wrap the data. 
}
```  

## Advanced Uses
Detailed usage has been documented in the wiki. Feel free to create an issue if any question is not answered.

## Why Use DbAutoFill
It helps keeping your code minimal, avoid repetitive task (catching exceptions), easy to refactor when changes are needed.

## Requirements
.NET Framework 4.5+ 

## License 
Licensed under MIT. See LICENSE file for more details.
