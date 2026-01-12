using Microsoft.AspNetCore.Mvc;
using BlueCloudApi.Models;

namespace BlueCloudApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _context;

    public TasksController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public ActionResult<IEnumerable<TaskModel>> GetTasks()
    {
        return _context.Tasks.ToList();
    }

    [HttpGet("{id}")]
    public ActionResult<TaskModel> GetTask(int id)
    {
        var task = _context.Tasks.Find(id);
        if (task == null)
        {
            return NotFound();
        }
        return task;
    }

    [HttpPost]
    public ActionResult<TaskModel> CreateTask(TaskModel task)
    {
        _context.Tasks.Add(task);
        _context.SaveChanges();
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateTask(int id, TaskModel task)
    {
        if (id != task.Id)
        {
            return BadRequest();
        }

        _context.Entry(task).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        _context.SaveChanges();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteTask(int id)
    {
        var task = _context.Tasks.Find(id);
        if (task == null)
        {
            return NotFound();
        }

        _context.Tasks.Remove(task);
        _context.SaveChanges();

        return NoContent();
    }
}
