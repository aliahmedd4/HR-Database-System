/* ═══════════════════════════════════════════
   HRMS Admin Dashboard — JS
   ═══════════════════════════════════════════ */
(function () {
  'use strict';

  var body = document.body;

  /* ── Sidebar toggle ── */
  document.querySelectorAll('.topbar-toggle').forEach(function (btn) {
    btn.addEventListener('click', function () {
      if (window.innerWidth <= 768) {
        body.classList.toggle('sidebar-open');
      } else {
        var collapsed = body.classList.toggle('sidebar-collapsed');
        try { localStorage.setItem('hrms_sidebar_collapsed', collapsed ? '1' : '0'); } catch (e) {}
      }
    });
  });

  // Restore collapsed state on desktop
  if (window.innerWidth > 768) {
    try {
      if (localStorage.getItem('hrms_sidebar_collapsed') === '1') {
        body.classList.add('sidebar-collapsed');
      }
    } catch (e) {}
  }

  // Close mobile sidebar when overlay is clicked
  document.addEventListener('click', function (e) {
    if (window.innerWidth <= 768 && body.classList.contains('sidebar-open')) {
      if (!e.target.closest('.admin-sidebar') && !e.target.closest('.topbar-toggle')) {
        body.classList.remove('sidebar-open');
      }
    }
  });

  /* ── Active nav link ── */
  var currentPath = window.location.pathname.toLowerCase();
  document.querySelectorAll('.admin-sidebar .nav-item a').forEach(function (link) {
    var href = (link.getAttribute('href') || '').toLowerCase();
    if (href && href !== '/' && currentPath.indexOf(href) === 0) {
      link.classList.add('active');
    }
    // Exact match for dashboard
    if (href === '/' && currentPath === '/') {
      link.classList.add('active');
    }
  });

  /* ══════════════════════
     TOAST SYSTEM
  ══════════════════════ */
  var toastArea = document.getElementById('toastArea');

  window.showToast = function (type, message, title) {
    if (!toastArea) return;
    var iconMap = {
      success: 'bi-check-circle-fill',
      error:   'bi-x-circle-fill',
      info:    'bi-info-circle-fill',
      warning: 'bi-exclamation-triangle-fill'
    };
    var defaultTitle = { success: 'Success', error: 'Error', info: 'Info', warning: 'Warning' };

    var item = document.createElement('div');
    item.className = 'toast-item toast-' + type;
    item.innerHTML =
      '<i class="bi ' + (iconMap[type] || 'bi-info-circle-fill') + ' toast-icon"></i>' +
      '<div class="toast-body">' +
        '<div class="toast-title">' + (title || defaultTitle[type] || type) + '</div>' +
        '<div class="toast-msg">' + message + '</div>' +
      '</div>' +
      '<button class="toast-close" aria-label="Close"><i class="bi bi-x"></i></button>';

    item.querySelector('.toast-close').addEventListener('click', function () {
      item.remove();
    });

    toastArea.appendChild(item);
    setTimeout(function () { if (item.parentNode) item.remove(); }, 5500);
  };

  // Auto-show TempData messages as toasts.
  // Guard: if the view already rendered a Bootstrap alert with the same text,
  // skip the toast to avoid double feedback (old views consume TempData first
  // then _Layout also reads the same value within the same request).
  function bootstrapAlertExists(text) {
    var alerts = document.querySelectorAll('.alert-success, .alert-danger, .alert-warning');
    for (var i = 0; i < alerts.length; i++) {
      if (alerts[i].textContent.trim().indexOf(text.trim()) !== -1) return true;
    }
    return false;
  }

  var tempSuccess = document.getElementById('tempSuccess');
  var tempError   = document.getElementById('tempError');
  if (tempSuccess && tempSuccess.textContent.trim()) {
    var msg = tempSuccess.textContent.trim();
    if (!bootstrapAlertExists(msg)) showToast('success', msg);
  }
  if (tempError && tempError.textContent.trim()) {
    var emsg = tempError.textContent.trim();
    if (!bootstrapAlertExists(emsg)) showToast('error', emsg);
  }

  /* ══════════════════════
     DATA TABLE HELPERS
  ══════════════════════ */
  document.querySelectorAll('[data-dt-search]').forEach(function (input) {
    var tableId = input.getAttribute('data-dt-search');
    var table   = document.getElementById(tableId);
    if (!table) return;
    var rows = Array.from(table.querySelectorAll('tbody tr'));
    var pageSize = parseInt(input.getAttribute('data-dt-pagesize') || '10', 10);
    var currentPage = 1;
    var infoEl  = document.querySelector('[data-dt-info="' + tableId + '"]');
    var pageNav = document.querySelector('[data-dt-pages="' + tableId + '"]');

    function filterRows() {
      var q = input.value.toLowerCase();
      rows.forEach(function (r) {
        r.setAttribute('data-match', r.textContent.toLowerCase().includes(q) ? '1' : '0');
      });
      currentPage = 1;
      renderPage();
    }

    function renderPage() {
      var visible = rows.filter(function (r) { return r.getAttribute('data-match') !== '0'; });
      var total   = visible.length;
      var pages   = Math.max(1, Math.ceil(total / pageSize));
      currentPage = Math.min(currentPage, pages);
      var start   = (currentPage - 1) * pageSize;

      rows.forEach(function (r) { r.style.display = 'none'; });
      visible.slice(start, start + pageSize).forEach(function (r) { r.style.display = ''; });

      if (infoEl) {
        infoEl.textContent = total === 0
          ? 'No records found'
          : 'Showing ' + (start + 1) + '–' + Math.min(start + pageSize, total) + ' of ' + total;
      }

      if (pageNav) {
        pageNav.innerHTML = '';
        // Prev
        var prev = makePageBtn('<i class="bi bi-chevron-left"></i>', currentPage > 1);
        prev.addEventListener('click', function () { if (currentPage > 1) { currentPage--; renderPage(); } });
        pageNav.appendChild(prev);
        // Page numbers (max 5 shown)
        var startP = Math.max(1, currentPage - 2);
        var endP   = Math.min(pages, startP + 4);
        for (var p = startP; p <= endP; p++) {
          (function (pg) {
            var btn = makePageBtn(pg, true);
            if (pg === currentPage) btn.classList.add('active');
            btn.addEventListener('click', function () { currentPage = pg; renderPage(); });
            pageNav.appendChild(btn);
          })(p);
        }
        // Next
        var next = makePageBtn('<i class="bi bi-chevron-right"></i>', currentPage < pages);
        next.addEventListener('click', function () { if (currentPage < pages) { currentPage++; renderPage(); } });
        pageNav.appendChild(next);
      }
    }

    function makePageBtn(label, enabled) {
      var btn = document.createElement('button');
      btn.className = 'dt-page-btn';
      btn.innerHTML = label;
      if (!enabled) btn.disabled = true;
      return btn;
    }

    input.addEventListener('input', filterRows);
    // Initialize
    rows.forEach(function (r) { r.setAttribute('data-match', '1'); });
    renderPage();
  });

  /* ── Column sort ── */
  document.querySelectorAll('table.admin-table th[data-sort]').forEach(function (th) {
    th.style.cursor = 'pointer';
    th.addEventListener('click', function () {
      var table  = th.closest('table');
      var tbody  = table.querySelector('tbody');
      var idx    = Array.from(th.parentNode.children).indexOf(th);
      var asc    = th.getAttribute('data-sort-dir') !== 'asc';
      th.setAttribute('data-sort-dir', asc ? 'asc' : 'desc');
      table.querySelectorAll('th[data-sort]').forEach(function (t) { t.removeAttribute('data-sort-dir'); });
      th.setAttribute('data-sort-dir', asc ? 'asc' : 'desc');

      var rows2 = Array.from(tbody.querySelectorAll('tr'));
      rows2.sort(function (a, b) {
        var av = (a.children[idx] || {}).textContent.trim().toLowerCase();
        var bv = (b.children[idx] || {}).textContent.trim().toLowerCase();
        return asc ? av.localeCompare(bv) : bv.localeCompare(av);
      });
      rows2.forEach(function (r) { tbody.appendChild(r); });
    });
  });

  /* ── Global search (topbar) — filter visible page tables ── */
  var globalSearch = document.getElementById('globalSearch');
  if (globalSearch) {
    globalSearch.addEventListener('input', function () {
      var q = this.value.toLowerCase().trim();
      if (!q) return;
      // Highlight matching dt-search if present
      var dtInput = document.querySelector('[data-dt-search]');
      if (dtInput) { dtInput.value = q; dtInput.dispatchEvent(new Event('input')); }
    });
  }

})();
