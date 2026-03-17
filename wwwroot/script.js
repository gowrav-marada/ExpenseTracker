/* ???????????????????????????????????????????????????
   Expense Tracker – Dashboard Script
   ??????????????????????????????????????????????????? */

const API_BASE = window.location.origin;
const PAGE_SIZE = 10;

let currentPage = 1;
let categoriesCache = [];

/* ?? Auth Guard ?? */
(function guard() {
    if (!localStorage.getItem('et_token')) {
        window.location.href = 'index.html';
    }
})();

/* ??? Helpers ??? */
function authHeaders() {
    return {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('et_token')}`
    };
}

async function api(path, options = {}) {
    const res = await fetch(`${API_BASE}${path}`, {
        ...options,
        headers: { ...authHeaders(), ...(options.headers || {}) }
    });
    if (res.status === 401) {
        localStorage.removeItem('et_token');
        localStorage.removeItem('et_user');
        window.location.href = 'index.html';
        return;
    }
    if (res.status === 204) return null;
    if (!res.ok) {
        const err = await res.json().catch(() => ({}));
        throw new Error(err.message || `Request failed (${res.status})`);
    }
    return res.json();
}

function formatCurrency(amount, currency = 'INR') {
    return new Intl.NumberFormat('en-IN', { style: 'currency', currency }).format(amount);
}

function formatDate(dateStr) {
    return new Date(dateStr).toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' });
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text || '';
    return div.innerHTML;
}

/* ?? Toast ?? */
function toast(message, type = 'success') {
    const container = document.getElementById('toast-container');
    const el = document.createElement('div');
    el.className = `toast ${type}`;
    el.textContent = message;
    container.appendChild(el);
    setTimeout(() => { el.style.opacity = '0'; el.style.transform = 'translateY(12px)'; setTimeout(() => el.remove(), 300); }, 3000);
}

/* ?? INITIALIZATION ?? */
document.addEventListener('DOMContentLoaded', () => {
    loadUserInfo();
    setDefaultDates();
    loadCategories().then(() => {
        loadDashboard();
    });
});

function loadUserInfo() {
    const user = JSON.parse(localStorage.getItem('et_user') || '{}');
    const avatarEl = document.getElementById('sidebar-avatar');
    if (user.picture) {
        avatarEl.innerHTML = `<img src="${user.picture}" alt="" />`;
    } else {
        avatarEl.textContent = (user.name || '?')[0];
    }
    document.getElementById('sidebar-name').textContent = user.name || 'User';
    document.getElementById('sidebar-email').textContent = user.email || '';

    const hour = new Date().getHours();
    let greeting = 'Good evening';
    if (hour < 12) greeting = 'Good morning';
    else if (hour < 18) greeting = 'Good afternoon';
    document.getElementById('greeting').textContent = `${greeting}, ${(user.name || 'there').split(' ')[0]}!`;
}

function setDefaultDates() {
    const now = new Date();
    const firstDay = new Date(now.getFullYear(), now.getMonth(), 1);
    document.getElementById('summary-from').value = firstDay.toISOString().split('T')[0];
    document.getElementById('summary-to').value = now.toISOString().split('T')[0];
}

function signOut() {
    localStorage.removeItem('et_token');
    localStorage.removeItem('et_user');
    window.location.href = 'index.html';
}

/* ?? NAVIGATION ?? */
function switchView(view) {
    document.querySelectorAll('.view').forEach(v => v.classList.remove('active'));
    document.getElementById(`view-${view}`).classList.add('active');

    document.querySelectorAll('.nav-btn').forEach(b => b.classList.remove('active'));
    document.querySelector(`.nav-btn[data-view="${view}"]`).classList.add('active');

    if (view === 'expenses') loadExpenses();
    if (view === 'categories') renderCategories();
    if (view === 'dashboard') loadDashboard();

    // Close mobile sidebar
    document.getElementById('sidebar').classList.remove('open');
    document.getElementById('sidebar-overlay').classList.remove('open');
}

function toggleSidebar() {
    document.getElementById('sidebar').classList.toggle('open');
    document.getElementById('sidebar-overlay').classList.toggle('open');
}

/* ?? CATEGORIES ?? */
async function loadCategories() {
    try {
        categoriesCache = await api('/api/categories');
        populateCategoryDropdowns();
    } catch (e) {
        toast(e.message, 'error');
    }
}

function populateCategoryDropdowns() {
    // Expense form
    const expCat = document.getElementById('expense-category');
    expCat.innerHTML = '<option value="">Select category</option>' +
        categoriesCache.map(c => `<option value="${c.id}">${c.icon || '??'} ${escapeHtml(c.name)}</option>`).join('');

    // Filter
    const filterCat = document.getElementById('filter-category');
    const currentVal = filterCat.value;
    filterCat.innerHTML = '<option value="">All Categories</option>' +
        categoriesCache.map(c => `<option value="${c.id}">${c.icon || '??'} ${escapeHtml(c.name)}</option>`).join('');
    filterCat.value = currentVal;
}

function getCategoryById(id) {
    return categoriesCache.find(c => c.id === id);
}

function renderCategories() {
    const grid = document.getElementById('categories-grid');
    const empty = document.getElementById('categories-empty');

    if (categoriesCache.length === 0) {
        grid.innerHTML = '';
        empty.classList.remove('hidden');
        return;
    }

    empty.classList.add('hidden');
    grid.innerHTML = categoriesCache.map(c => `
        <div class="category-card">
            <div class="category-icon-box" style="background: ${c.color || '#6366f1'}22; color: ${c.color || '#6366f1'}">
                ${c.icon || '??'}
            </div>
            <div class="category-info">
                <div class="category-name">${escapeHtml(c.name)}</div>
                <div class="category-date">Created ${formatDate(c.createdAt)}</div>
            </div>
            <div class="category-actions">
                <button class="btn-icon" title="Edit" onclick="editCategory('${c.id}')">??</button>
                <button class="btn-icon danger" title="Delete" onclick="confirmDeleteCategory('${c.id}', '${escapeHtml(c.name)}')">???</button>
            </div>
        </div>
    `).join('');
}

function openCategoryModal(editing = null) {
    document.getElementById('category-modal-title').textContent = editing ? 'Edit Category' : 'Add Category';
    document.getElementById('category-submit-btn').textContent = editing ? 'Update' : 'Save Category';
    document.getElementById('category-id').value = editing ? editing.id : '';
    document.getElementById('category-name').value = editing ? editing.name : '';
    document.getElementById('category-icon').value = editing ? (editing.icon || '') : '';
    document.getElementById('category-color').value = editing ? (editing.color || '#6366f1') : '#6366f1';
    document.getElementById('category-modal').classList.remove('hidden');
}

function closeCategoryModal() {
    document.getElementById('category-modal').classList.add('hidden');
    document.getElementById('category-form').reset();
}

function editCategory(id) {
    const cat = getCategoryById(id);
    if (cat) openCategoryModal(cat);
}

async function saveCategory(e) {
    e.preventDefault();
    const id = document.getElementById('category-id').value;
    const data = {
        name: document.getElementById('category-name').value.trim(),
        icon: document.getElementById('category-icon').value.trim() || null,
        color: document.getElementById('category-color').value
    };

    try {
        if (id) {
            await api(`/api/categories/${id}`, { method: 'PUT', body: JSON.stringify(data) });
            toast('Category updated');
        } else {
            await api('/api/categories', { method: 'POST', body: JSON.stringify(data) });
            toast('Category created');
        }
        closeCategoryModal();
        await loadCategories();
        renderCategories();
    } catch (e) {
        toast(e.message, 'error');
    }
}

function confirmDeleteCategory(id, name) {
    document.getElementById('delete-msg').textContent = `Delete category "${name}"? This cannot be undone.`;
    document.getElementById('delete-confirm-btn').onclick = () => deleteCategory(id);
    document.getElementById('delete-modal').classList.remove('hidden');
}

async function deleteCategory(id) {
    try {
        await api(`/api/categories/${id}`, { method: 'DELETE' });
        toast('Category deleted');
        closeDeleteModal();
        await loadCategories();
        renderCategories();
    } catch (e) {
        toast(e.message, 'error');
    }
}

/* ?? EXPENSES ?? */
async function loadExpenses() {
    const categoryId = document.getElementById('filter-category').value;
    const from = document.getElementById('filter-from').value;
    const to = document.getElementById('filter-to').value;

    const params = new URLSearchParams({ page: currentPage, pageSize: PAGE_SIZE });
    if (categoryId) params.set('categoryId', categoryId);
    if (from) params.set('from', from);
    if (to) params.set('to', to);

    try {
        const result = await api(`/api/expenses?${params}`);
        renderExpensesTable(result);
        renderPagination(result);
    } catch (e) {
        toast(e.message, 'error');
    }
}

function renderExpensesTable(result) {
    const tbody = document.getElementById('expenses-body');
    const empty = document.getElementById('expenses-empty');

    if (!result.items || result.items.length === 0) {
        tbody.innerHTML = '';
        empty.classList.remove('hidden');
        return;
    }

    empty.classList.add('hidden');
    tbody.innerHTML = result.items.map(exp => {
        const cat = getCategoryById(exp.categoryId);
        const catLabel = cat ? `<span class="expense-category-badge" style="background:${cat.color || '#6366f1'}18;color:${cat.color || '#a5b4fc'}">${cat.icon || '??'} ${escapeHtml(cat.name)}</span>` : `<span class="expense-category-badge">—</span>`;
        return `
        <tr>
            <td class="expense-title-cell" data-label="Title">${escapeHtml(exp.title)}</td>
            <td data-label="Category">${catLabel}</td>
            <td class="expense-amount" data-label="Amount">${formatCurrency(exp.amount, exp.currency)}</td>
            <td class="expense-date-cell" data-label="Date">${formatDate(exp.date)}</td>
            <td class="expense-note-cell" data-label="Note" title="${escapeHtml(exp.note)}">${escapeHtml(exp.note) || '—'}</td>
            <td data-label="">
                <div class="action-btns">
                    <button class="btn-icon" title="Edit" onclick="editExpense('${exp.id}')">??</button>
                    <button class="btn-icon danger" title="Delete" onclick="confirmDeleteExpense('${exp.id}', '${escapeHtml(exp.title)}')">???</button>
                </div>
            </td>
        </tr>`;
    }).join('');
}

function renderPagination(result) {
    const container = document.getElementById('pagination');
    if (result.totalPages <= 1) { container.innerHTML = ''; return; }

    let html = `<button class="page-btn" onclick="goToPage(${result.page - 1})" ${result.page <= 1 ? 'disabled' : ''}>‹</button>`;
    for (let i = 1; i <= result.totalPages; i++) {
        if (result.totalPages > 7 && Math.abs(i - result.page) > 2 && i !== 1 && i !== result.totalPages) {
            if (i === result.page - 3 || i === result.page + 3) html += `<span style="color:var(--text-muted);padding:0 .25rem">…</span>`;
            continue;
        }
        html += `<button class="page-btn ${i === result.page ? 'active' : ''}" onclick="goToPage(${i})">${i}</button>`;
    }
    html += `<button class="page-btn" onclick="goToPage(${result.page + 1})" ${result.page >= result.totalPages ? 'disabled' : ''}>›</button>`;
    container.innerHTML = html;
}

function goToPage(page) {
    currentPage = page;
    loadExpenses();
}

function clearFilters() {
    document.getElementById('filter-category').value = '';
    document.getElementById('filter-from').value = '';
    document.getElementById('filter-to').value = '';
    currentPage = 1;
    loadExpenses();
}

function openExpenseModal(editing = null) {
    document.getElementById('expense-modal-title').textContent = editing ? 'Edit Expense' : 'Add Expense';
    document.getElementById('expense-submit-btn').textContent = editing ? 'Update' : 'Save Expense';
    document.getElementById('expense-id').value = editing ? editing.id : '';
    document.getElementById('expense-title').value = editing ? editing.title : '';
    document.getElementById('expense-amount').value = editing ? editing.amount : '';
    document.getElementById('expense-currency').value = editing ? (editing.currency || 'INR') : 'INR';
    document.getElementById('expense-category').value = editing ? (editing.categoryId || '') : '';
    document.getElementById('expense-date').value = editing ? editing.date.split('T')[0] : new Date().toISOString().split('T')[0];
    document.getElementById('expense-note').value = editing ? (editing.note || '') : '';
    document.getElementById('expense-modal').classList.remove('hidden');
}

function closeExpenseModal() {
    document.getElementById('expense-modal').classList.add('hidden');
    document.getElementById('expense-form').reset();
}

async function editExpense(id) {
    try {
        const exp = await api(`/api/expenses/${id}`);
        openExpenseModal(exp);
    } catch (e) {
        toast(e.message, 'error');
    }
}

async function saveExpense(e) {
    e.preventDefault();
    const id = document.getElementById('expense-id').value;
    const data = {
        title: document.getElementById('expense-title').value.trim(),
        amount: parseFloat(document.getElementById('expense-amount').value),
        currency: document.getElementById('expense-currency').value,
        categoryId: document.getElementById('expense-category').value,
        date: document.getElementById('expense-date').value || null,
        note: document.getElementById('expense-note').value.trim() || null
    };

    try {
        if (id) {
            await api(`/api/expenses/${id}`, { method: 'PUT', body: JSON.stringify(data) });
            toast('Expense updated');
        } else {
            await api('/api/expenses', { method: 'POST', body: JSON.stringify(data) });
            toast('Expense added');
        }
        closeExpenseModal();
        loadExpenses();
        loadDashboard();
    } catch (e) {
        toast(e.message, 'error');
    }
}

function confirmDeleteExpense(id, title) {
    document.getElementById('delete-msg').textContent = `Delete expense "${title}"? This cannot be undone.`;
    document.getElementById('delete-confirm-btn').onclick = () => deleteExpense(id);
    document.getElementById('delete-modal').classList.remove('hidden');
}

async function deleteExpense(id) {
    try {
        await api(`/api/expenses/${id}`, { method: 'DELETE' });
        toast('Expense deleted');
        closeDeleteModal();
        loadExpenses();
        loadDashboard();
    } catch (e) {
        toast(e.message, 'error');
    }
}

function closeDeleteModal() {
    document.getElementById('delete-modal').classList.remove('hidden');
    document.getElementById('delete-modal').classList.add('hidden');
}

/* ?? DASHBOARD ?? */
async function loadDashboard() {
    const from = document.getElementById('summary-from').value;
    const to = document.getElementById('summary-to').value;

    try {
        const params = new URLSearchParams();
        if (from) params.set('from', from);
        if (to) params.set('to', to);

        params.set('currency', 'INR');
        const [summary, recent] = await Promise.all([
            api(`/api/expenses/summary?${params}`),
            api(`/api/expenses?page=1&pageSize=5`)
        ]);

        // Summary cards
        document.getElementById('total-amount').textContent = formatCurrency(summary.totalAmount, summary.currency);
        document.getElementById('total-count').textContent = summary.totalCount;
        document.getElementById('category-count').textContent = summary.byCategory?.length || 0;
        const avg = summary.totalCount > 0 ? summary.totalAmount / summary.totalCount : 0;
        document.getElementById('avg-amount').textContent = formatCurrency(avg, summary.currency);

        // Breakdown chart
        renderBreakdownChart(summary.byCategory || [], summary.currency);

        // Recent expenses
        renderRecentExpenses(recent.items || []);
    } catch (e) {
        toast(e.message, 'error');
    }
}

const BAR_COLORS = ['#6366f1', '#22c55e', '#f59e0b', '#ec4899', '#06b6d4', '#8b5cf6', '#ef4444', '#14b8a6', '#f97316', '#64748b'];

function renderBreakdownChart(categories, currency) {
    const container = document.getElementById('breakdown-chart');
    if (!categories.length) {
        container.innerHTML = '<p style="color:var(--text-muted);font-size:.85rem;text-align:center;padding:1rem 0">No spending data for this period</p>';
        return;
    }

    const maxAmount = Math.max(...categories.map(c => c.totalAmount));

    container.innerHTML = categories.map((c, i) => {
        const color = BAR_COLORS[i % BAR_COLORS.length];
        const pct = maxAmount > 0 ? (c.totalAmount / maxAmount) * 100 : 0;
        return `
        <div class="bar-row">
            <div class="bar-label" title="${escapeHtml(c.category)}">${escapeHtml(c.category)}</div>
            <div class="bar-track">
                <div class="bar-fill" style="width:${pct}%;background:${color}"></div>
            </div>
            <div class="bar-value">${formatCurrency(c.totalAmount, currency)}</div>
        </div>`;
    }).join('');
}

function renderRecentExpenses(items) {
    const container = document.getElementById('recent-list');
    if (!items.length) {
        container.innerHTML = '<p style="color:var(--text-muted);font-size:.85rem;text-align:center;padding:1.5rem 0">No recent expenses</p>';
        return;
    }

    container.innerHTML = items.map(exp => {
        const cat = getCategoryById(exp.categoryId);
        const icon = cat?.icon || '??';
        const color = cat?.color || '#6366f1';
        return `
        <div class="expense-mini-item">
            <div class="expense-mini-icon" style="background:${color}18;color:${color}">${icon}</div>
            <div class="expense-mini-info">
                <div class="expense-mini-title">${escapeHtml(exp.title)}</div>
                <div class="expense-mini-date">${formatDate(exp.date)}</div>
            </div>
            <div class="expense-mini-amount">${formatCurrency(exp.amount, exp.currency)}</div>
        </div>`;
    }).join('');
}

/* ?? Keyboard shortcuts ?? */
document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
        closeExpenseModal();
        closeCategoryModal();
        closeDeleteModal();
    }
});
