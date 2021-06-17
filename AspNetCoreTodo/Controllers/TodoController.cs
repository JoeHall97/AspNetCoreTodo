using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using AspNetCoreTodo.Services;
using AspNetCoreTodo.Models;

namespace AspNetCoreTodo.Controllers
{
  [Authorize]
  public class TodoController : Controller
  {
    private readonly ITodoItemService _todoItemService;
    private readonly UserManager<ApplicationUser> _userManager;

    public TodoController(ITodoItemService todoItemService, UserManager<ApplicationUser> userManager)
    {
      _todoItemService = todoItemService;
      _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
      var currentUser = await _userManager.GetUserAsync(User);
      if (currentUser == null) return Challenge();

      // get items from the service layer
      var items = await _todoItemService.GetIncompleteItemsAsync(currentUser);

      // put items into a model
      var model = new TodoViewModel()
      {
        Items = items
      };

      //pass the view to a model and render
      return View(model);
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(TodoItem newItem)
    {
      if(!ModelState.IsValid)
      {
        return RedirectToAction("Index");
      }

      var currentUser = await _userManager.GetUserAsync(User);
      if (currentUser == null) return Challenge();

      var successful = await _todoItemService.AddItemAsync(newItem, currentUser);
      if(!successful)
      {
        return BadRequest("Could not add item.");
      }

      return RedirectToAction("Index");
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkDone(Guid id)
    {
      if(id == Guid.Empty)
      {
        return RedirectToAction("Index");
      }

      var currentUser = await _userManager.GetUserAsync(User);
      if (currentUser == null) return Challenge();

      var successful = await _todoItemService.MarkDoneAsync(id, currentUser);
      if(!successful)
      {
        return BadRequest("Could not mark item as done.");
      }

      return RedirectToAction("Index");
    }
  }
}