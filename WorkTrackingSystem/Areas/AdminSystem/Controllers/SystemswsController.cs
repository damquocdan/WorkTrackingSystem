using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkTrackingSystem.Models;

namespace WorkTrackingSystem.Areas.AdminSystem.Controllers
{
    [Area("AdminSystem")]
    public class SystemswsController : BaseController
    {
        private readonly WorkTrackingSystemContext _context;

        public SystemswsController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        // GET: AdminSystem/Systemsws
        public async Task<IActionResult> Index()
        {
            return View(await _context.Systemsws.ToListAsync());
        }

        // GET: AdminSystem/Systemsws/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var systemsw = await _context.Systemsws
                .FirstOrDefaultAsync(m => m.Id == id);
            if (systemsw == null)
            {
                return NotFound();
            }

            return View(systemsw);
        }

        // GET: AdminSystem/Systemsws/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdminSystem/Systemsws/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Value,Description,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Systemsw systemsw)
        {
            if (ModelState.IsValid)
            {
                _context.Add(systemsw);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(systemsw);
        }

        // GET: AdminSystem/Systemsws/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var systemsw = await _context.Systemsws.FindAsync(id);
            if (systemsw == null)
            {
                return NotFound();
            }
            return View(systemsw);
        }

        // POST: AdminSystem/Systemsws/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Name,Value,Description,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Systemsw systemsw)
        {
            if (id != systemsw.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(systemsw);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SystemswExists(systemsw.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(systemsw);
        }

        // GET: AdminSystem/Systemsws/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var systemsw = await _context.Systemsws
                .FirstOrDefaultAsync(m => m.Id == id);
            if (systemsw == null)
            {
                return NotFound();
            }

            return View(systemsw);
        }

        // POST: AdminSystem/Systemsws/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var systemsw = await _context.Systemsws.FindAsync(id);
            if (systemsw != null)
            {
                _context.Systemsws.Remove(systemsw);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SystemswExists(long id)
        {
            return _context.Systemsws.Any(e => e.Id == id);
        }
    }
}
