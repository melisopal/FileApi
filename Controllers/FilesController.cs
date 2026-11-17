// using Microsoft.AspNetCore.Mvc;

//  // Attribute indicates that controller responds to web API requests
// [ApiController] 
// // Attribute "this controller handles requests whose route starts with 'files'"
// [Route("files")] 

// // Important that static is here otherwise ASP.NET will create a brand new -
// // FileController for every request.
// // Also : ControllerBase means "inherit from ControllerBase" which is the -
// // same as extends in Java.
// public class FilesController : ControllerBase
// {
//     // Underscore in _files is convention for private fields.
//     // new() is shorthand for new List<FileRecord>().
//     private static readonly List<FileRecord> _files = new();
//     // Next ID for new FileRecord, simple way to generate IDs for in-memory list.
//     private static int _nextId = 1;

//     // Attribute that says this method responds to HTTP GET requests.
//     [HttpGet]
//     public ActionResult<List<FileRecord>> GetAll()
//     {
//         // Is one of the helper methods from ControllerBase that - 
//         // returns a 200 OK response with the given data.
//         return Ok(_files);

//         /*
//         _files.Add(new FileRecord { Name = "test.txt" });
//         _files = new List<FileRecord>();
//         */
//         // Commented out code to test readonly rules.
//     }

//     // Attribute that says this method responds to HTTP GET requests with an ID parameter.
//     // Like /files/5 matches it with id = 5.
//     [HttpGet("{id}")]

//     // Returns a response containing *one* FileRecord.
//     // int id is input, ASP.NET sees the name id matches {id} in address.
//     public ActionResult<FileRecord> GetById(int id)
//     {
//         /// <summary>
//         /// 
//         /// var means "figure out the type for me", in this case it will be FileRecord or null.
//         /// 
//         /// FirstOrDefault returns first element of a collection matching a condition, -
//         /// or null if none match.
//         /// 
//         /// f => f.Id == id is the matching rule called lambda expression. It says -
//         /// "for each f in the collection, check if f.Id equals the input id".
//         /// 
//         var record = _files.FirstOrDefault(f => f.Id == id);

//         if (record == null)
//         {
//             return NotFound();
//         }
//         return Ok(record);
//     }

//     // Attribute that says this method responds to HTTP POST requests.
//     [HttpPost]
//     // FileRecord input is the caller sends.
//     public ActionResult<FileRecord> Create(FileRecord input)
//     {
//         // Record gets the current value of _nextId, then _nextId is incremented for the next record.
//         input.Id = _nextId++;
//         input.CreatedAt = DateTime.UtcNow;
//         _files.Add(input);
//         /// This replies with status 201 Created which is proper response for "I made a new thing".
//         /// new { id = input.Id } supplies the id so ASP.NET can build address /files/5
//         /// 
//         return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
//     }

//     [HttpPut("{id}")]
//     // IActionResult is a generic response type that can return different HTTP status codes.
//     public IActionResult Update(int id, FileRecord input)
//     {
//         var record = _files.FirstOrDefault(f => f.Id == id);
        
//         if (record == null)
//         {
//             return NotFound();
//         }

//         // Update the record's name with the new value from input.
//         // The only thing we can do, since id and date aren't things users should be able to change.
//         record.Name = input.Name;
//         return NoContent();
//     }

//     [HttpDelete("{id}")]
    
//     public IActionResult Delete(int id)
//     {
//         var record = _files.FirstOrDefault(f => f.Id == id);
       
//         if (record == null)
//         {
//             return NotFound();
//         }
        
//         // Remove record allowed here as _files is readonly.
//         _files.Remove(record);
//         return NoContent();
//     }

// }

/*
Caller  →  API (controller)  →  EF Core  →  SQL Server
        ←                    ←           ←

Each layer has one job:

Controller: understand the request, decide what to do, pick the right status code
EF Core: translate C# into SQL and back
SQL Server: store the data permanently

Before the switch, the controller kept its data in a C# list. That list lives 
in app’s memory, and the database never heard about it.
API worked perfectly, it just wasn’t connected to anything. Creating the 
database and table was like building a warehouse that nobody was sending boxes to.
*/

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("files")]
public class FilesController : ControllerBase
{
    private readonly AppDbContext _db;

    public FilesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public ActionResult<List<FileRecord>> GetAll()
    {
        return Ok(_db.Files.ToList());
    }

    [HttpGet("{id}")]
    public ActionResult<FileRecord> GetById(int id)
    {
        var record = _db.Files.Find(id);
        if (record == null)
        {
            return NotFound();
        }
        return Ok(record);
    }

    [HttpPost]
    public ActionResult<FileRecord> Create(FileRecord input)
    {
        input.Id = 0;
        input.CreatedAt = DateTime.UtcNow;
        _db.Files.Add(input);
        _db.SaveChanges();
        return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, FileRecord input)
    {
        var record = _db.Files.Find(id);
        if (record == null)
        {
            return NotFound();
        }
        record.Name = input.Name;
        _db.SaveChanges();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var record = _db.Files.Find(id);
        if (record == null)
        {
            return NotFound();
        }
        _db.Files.Remove(record);
        _db.SaveChanges();
        return NoContent();
    }
}