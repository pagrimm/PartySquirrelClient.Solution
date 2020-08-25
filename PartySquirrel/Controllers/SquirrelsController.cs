using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PartySquirrel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using PartySquirrel.ViewModels;

namespace PartySquirrel.Controllers
{
  public class SquirrelsController : Controller
  {
    private readonly PartySquirrelContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    public SquirrelsController(PartySquirrelContext db, UserManager<ApplicationUser> userManager)
    {
      _db = db;
      _userManager = userManager;
    } 
    public IActionResult Index() //gonutsnonuts page
    {
      var result = Src.GetPhoto();
      return View("Index", result);
    }

    public IActionResult Create(string url) // form to add details
    {
      ViewBag.Url = url;
      return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Squirrel squirrel, string url)
    {
      var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var currentUser = await _userManager.FindByIdAsync(userId);
      squirrel.Image = url;
      _db.Squirrels.Add(squirrel);

      _db.SquirrelUser.Add(new SquirrelUser(){SquirrelId = squirrel.SquirrelId, UserId = userId, Squirrel = squirrel, User = currentUser});
      _db.SaveChanges();
      return RedirectToAction("Details", "Parties", new { id = userId } );
    }

    public IActionResult Details(int id) //squirrel details page
    {
      var thisSquirrel = _db.Squirrels.FirstOrDefault(squirrel => squirrel.SquirrelId == id);
      return View(thisSquirrel);
    }

    public IActionResult Edit(int id)
    {
      var squirrelChange = _db.Squirrels.FirstOrDefault(squirrel => squirrel.SquirrelId == id);
      return View(squirrelChange);
    }

    [HttpPost]
    public ActionResult Edit(Squirrel squirrel)
    {
      var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      _db.Entry(squirrel).State = EntityState.Modified;
      _db.SaveChanges();
      return RedirectToAction("Details", "Parties", new { id = userId });
    }

    public IActionResult Delete (int id)
    {
      var thisSquirrel = _db.Squirrels.FirstOrDefault(squirrel => squirrel.SquirrelId == id);
      return View(thisSquirrel);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirm (int id)
    {
      var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var joinList = _db.SquirrelUser.Where(entry => entry.SquirrelId == id).ToList();
      var joinEntry = _db.SquirrelUser.FirstOrDefault(entry => entry.SquirrelId == id && entry.UserId == userId);
      if (joinList.Count == 1)
      {
        var thisSquirrel = _db.Squirrels.FirstOrDefault(squirrel => squirrel.SquirrelId == joinEntry.SquirrelId);
        _db.Squirrels.Remove(thisSquirrel);
      }
      _db.SquirrelUser.Remove(joinEntry);
      _db.SaveChanges();
      return RedirectToAction("Details", "Parties", new { id = userId });
    }
  }
}