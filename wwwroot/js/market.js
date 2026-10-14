(() => {
    const state = { user: null, products: [], categories: [], cart: new Map(), authMode: "login" };
    const byId = (id) => document.getElementById(id);
    const money = (value) => new Intl.NumberFormat("id-ID", { style: "currency", currency: "IDR", maximumFractionDigits: 0 }).format(value || 0);
    const today = new Date().toISOString().slice(0, 10);
    const monthAgo = new Date(Date.now() - 29 * 86400000).toISOString().slice(0, 10);

    async function api(url, options = {}) {
        const response = await fetch(url, {
            credentials: "same-origin",
            ...options,
            headers: { ...(options.body ? { "Content-Type": "application/json" } : {}), ...options.headers }
        });
        const body = response.status === 204 ? null : await response.json().catch(() => null);
        if (!response.ok) throw new Error(body?.message || `Request failed (${response.status})`);
        return body;
    }

    function node(tag, className, text) {
        const element = document.createElement(tag);
        if (className) element.className = className;
        if (text !== undefined) element.textContent = text;
        return element;
    }

    function button(text, className, action) {
        const element = node("button", className, text);
        element.type = "button";
        element.addEventListener("click", action);
        return element;
    }

    function setMessage(target, text, isError = false) {
        target.textContent = text || "";
        target.classList.toggle("is-error", isError);
    }

    function setConnection(connected) {
        const status = byId("connection-state");
        status.lastChild.textContent = connected ? " Connected" : " Offline";
        status.classList.toggle("is-offline", !connected);
    }

    async function loadCatalog() {
        [state.products, state.categories] = await Promise.all([
            api("/api/products"), api("/api/categories")
        ]);
        setConnection(true);
        renderStoreProducts();
        renderProductTable();
        renderCategoryOptions();
        byId("catalog-count").textContent = `${state.products.length} produk`;
    }

    function renderStoreProducts() {
        const target = state.user?.role === "Customer" ? byId("store-products") : byId("public-products");
        if (!target) return;
        target.replaceChildren();
        for (const product of state.products) {
            const row = node("article", "product-row");
            const details = node("div", "product-details");
            details.append(node("span", "product-category", product.category?.name || "Produk"));
            details.append(node("h3", "", product.name));
            details.append(node("span", "product-stock", `${product.stockQuantity} tersedia`));
            const price = node("strong", "product-price", money(product.price));
            row.append(details, price);
            if (state.user?.role === "Customer") {
                row.append(button("Tambah", "button button-outline add-button", () => addToCart(product)));
            }
            target.append(row);
        }
        if (!state.products.length) target.append(node("p", "empty-state", "Belum ada produk."));
    }

    function addToCart(product) {
        const current = state.cart.get(product.id)?.quantity || 0;
        if (current >= product.stockQuantity) {
            setMessage(byId("cart-message"), "Jumlah melebihi stok yang tersedia.", true);
            return;
        }
        state.cart.set(product.id, { product, quantity: current + 1 });
        setMessage(byId("cart-message"), "");
        renderCart();
    }

    function renderCart() {
        const target = byId("cart-items");
        target.replaceChildren();
        let count = 0;
        let total = 0;
        for (const [id, line] of state.cart) {
            count += line.quantity;
            total += line.product.price * line.quantity;
            const row = node("div", "cart-line");
            const info = node("div", "");
            info.append(node("strong", "", line.product.name));
            info.append(node("span", "", `${line.quantity} × ${money(line.product.price)}`));
            row.append(info, button("×", "icon-button remove-line", () => {
                const next = state.cart.get(id).quantity - 1;
                if (next) state.cart.get(id).quantity = next;
                else state.cart.delete(id);
                renderCart();
            }));
            target.append(row);
        }
        if (!count) target.append(node("p", "empty-state", "Keranjang masih kosong."));
        byId("cart-count").textContent = String(count);
        byId("cart-total").textContent = money(total);
        byId("checkout-button").disabled = !count;
    }

    function renderCategoryOptions() {
        const select = byId("product-category");
        if (!select) return;
        select.replaceChildren();
        for (const category of state.categories) {
            const option = node("option", "", category.name);
            option.value = category.id;
            select.append(option);
        }
    }

    function renderProductTable() {
        const target = byId("admin-products");
        if (!target || !state.user || state.user.role !== "Admin") return;
        target.replaceChildren();
        const table = node("table", "data-table");
        const head = node("thead", "");
        const header = node("tr", "");
        for (const title of ["Produk", "Harga", "Stok", ""]) header.append(node("th", "", title));
        head.append(header);
        const body = node("tbody", "");
        for (const product of state.products) {
            const row = node("tr", "");
            row.append(node("td", "", product.name));
            row.append(node("td", "", money(product.price)));
            row.append(node("td", "", String(product.stockQuantity)));
            const actions = node("td", "table-actions");
            actions.append(button("Ubah", "button button-text", () => editProduct(product)));
            actions.append(button("Hapus", "button button-text danger-text", () => deleteProduct(product)));
            row.append(actions);
            body.append(row);
        }
        table.append(head, body);
        target.append(table);
    }

    function editProduct(product) {
        const form = byId("product-form");
        for (const key of ["id", "name", "price", "stockQuantity", "quantityInPackage", "unitOfMeasurement", "categoryId"]) {
            form.elements[key].value = product[key];
        }
        byId("product-form-title").textContent = "Ubah produk";
        form.scrollIntoView({ behavior: "smooth", block: "center" });
    }

    async function deleteProduct(product) {
        if (!window.confirm(`Hapus ${product.name}?`)) return;
        try {
            await api(`/api/products/${product.id}`, { method: "DELETE" });
            await loadCatalog();
        } catch (error) {
            setMessage(byId("app-message"), error.message, true);
        }
    }

    function renderOrders(orders, target, staff) {
        target.replaceChildren();
        if (!orders.length) {
            target.append(node("p", "empty-state", "Belum ada pesanan."));
            return;
        }
        const table = node("table", "data-table");
        const head = node("thead", "");
        const header = node("tr", "");
        for (const title of staff ? ["Pesanan", "Pelanggan", "Tanggal", "Total", "Status", "Pembayaran", "Aksi"] : ["Pesanan", "Tanggal", "Total", "Status", "Aksi"])
            header.append(node("th", "", title));
        head.append(header);
        const body = node("tbody", "");
        for (const order of orders) {
            const row = node("tr", "");
            row.append(node("td", "order-number", order.orderNumber));
            if (staff) row.append(node("td", "", order.customerEmail || "—"));
            row.append(node("td", "", new Date(order.createdAtUtc).toLocaleDateString("id-ID")));
            row.append(node("td", "", money(order.total)));
            row.append(node("td", "", order.status));
            if (staff) row.append(node("td", "", order.paymentStatus));
            const actions = node("td", "table-actions");
            if (staff) {
                const next = { Pending: ["Confirmed", "Cancelled"], Confirmed: ["Shipped", "Cancelled"], Shipped: ["Delivered"] }[order.status] || [];
                for (const status of next) actions.append(button(status === "Confirmed" ? "Konfirmasi" : status === "Cancelled" ? "Batalkan" : status === "Shipped" ? "Kirim" : "Selesai", "button button-text", () => updateOrder(order.id, "status", status)));
                if (order.paymentStatus !== "Paid" && order.status === "Delivered") actions.append(button("Tandai lunas", "button button-text", () => updateOrder(order.id, "payment")));
            } else if (order.status === "Pending") {
                actions.append(button("Batalkan", "button button-text danger-text", () => cancelOrder(order.id)));
            }
            row.append(actions);
            body.append(row);
        }
        table.append(head, body);
        target.append(table);
    }

    async function updateOrder(id, action, status) {
        try {
            await api(`/api/orders/${id}/${action}`, {
                method: "PUT",
                body: action === "status" ? JSON.stringify({ status }) : undefined
            });
            await loadStaff();
        } catch (error) {
            setMessage(byId("app-message"), error.message, true);
        }
    }

    async function cancelOrder(id) {
        try {
            await api(`/api/orders/${id}/cancel`, { method: "POST" });
            await loadCustomerOrders();
            await loadCatalog();
        } catch (error) {
            setMessage(byId("app-message"), error.message, true);
        }
    }

    function currentRange() {
        return `fromDate=${byId("report-from").value}&toDate=${byId("report-to").value}`;
    }

    function renderSimpleTable(target, headings, rows) {
        target.replaceChildren();
        const table = node("table", "data-table");
        const head = node("thead", "");
        const header = node("tr", "");
        headings.forEach(item => header.append(node("th", "", item)));
        head.append(header);
        const body = node("tbody", "");
        for (const values of rows) {
            const row = node("tr", "");
            values.forEach(value => row.append(node("td", "", String(value))));
            body.append(row);
        }
        table.append(head, body);
        target.append(table);
    }

    async function loadReports() {
        const query = currentRange();
        const threshold = byId("report-threshold").value || "5";
        const [stock, sales, orders] = await Promise.all([
            api(`/api/reports/stock?threshold=${encodeURIComponent(threshold)}`),
            api(`/api/reports/sales?${query}`),
            api(`/api/reports/orders?${query}`)
        ]);
        byId("metric-low").textContent = String(stock.products.filter(item => item.stockQuantity <= stock.lowStockThreshold).length);
        byId("metric-revenue").textContent = money(sales.revenue);
        const summary = byId("report-summary");
        summary.replaceChildren();
        for (const [label, value] of [["Nilai stok", money(stock.inventoryValue)], ["Unit tersedia", stock.totalUnits], ["Pesanan lunas", sales.paidOrderCount], ["Penjualan", money(sales.revenue)]]) {
            const item = node("div", "report-stat");
            item.append(node("span", "", label), node("strong", "", String(value)));
            summary.append(item);
        }
        renderSimpleTable(byId("stock-report"), ["Produk", "Kategori", "Stok", "Nilai"], stock.products.map(item => [item.productName, item.categoryName, item.stockQuantity, money(item.stockValue)]));
        renderSimpleTable(byId("sales-report"), ["Tanggal UTC", "Pesanan", "Penjualan"], sales.daily.map(item => [new Date(item.dateUtc).toLocaleDateString("id-ID", { timeZone: "UTC" }), item.orderCount, money(item.revenue)]));
        renderSimpleTable(byId("order-report"), ["Status", "Jumlah"], Object.entries(orders.countsByStatus));
    }

    async function loadCustomerOrders() {
        const orders = await api("/api/orders");
        renderOrders(orders, byId("customer-orders"), false);
    }

    async function loadStaff() {
        const [orders, stock, sales] = await Promise.all([
            api("/api/orders"), api("/api/reports/stock"), api(`/api/reports/sales?fromDate=${monthAgo}&toDate=${today}`)
        ]);
        byId("metric-open").textContent = String(orders.filter(order => ["Pending", "Confirmed", "Shipped"].includes(order.status)).length);
        byId("metric-low").textContent = String(stock.products.filter(item => item.stockQuantity <= stock.lowStockThreshold).length);
        byId("metric-revenue").textContent = money(sales.revenue);
        renderOrders(orders, byId("staff-orders"), true);
        await loadCatalog();
    }

    async function showWorkspace(user) {
        state.user = user;
        byId("auth-view").hidden = true;
        document.querySelector(".public-catalog").hidden = true;
        byId("app-view").hidden = false;
        byId("account-email").textContent = user.email;
        byId("role-label").textContent = user.role.toUpperCase();
        byId("customer-workspace").hidden = user.role !== "Customer";
        byId("staff-workspace").hidden = user.role === "Customer";
        byId("page-title").textContent = user.role === "Customer" ? "Market untuk Anda" : "Ikhtisar Market";
        byId("category-form").hidden = user.role !== "Admin";
        byId("product-form").hidden = user.role !== "Admin";
        await loadCatalog();
        if (user.role === "Customer") {
            renderCart();
            await loadCustomerOrders();
        } else {
            await loadStaff();
            await loadReports();
        }
    }

    async function refreshUser() {
        try {
            const user = await api("/api/auth/me");
            await showWorkspace(user);
        } catch {
            state.user = null;
            setConnection(true);
        }
    }

    document.querySelectorAll("[data-auth-mode]").forEach(tab => tab.addEventListener("click", () => {
        state.authMode = tab.dataset.authMode;
        document.querySelectorAll("[data-auth-mode]").forEach(item => item.classList.toggle("selected", item === tab));
        byId("auth-title").textContent = state.authMode === "login" ? "Selamat datang kembali" : "Buat akun pembeli";
        byId("auth-submit").firstChild.textContent = state.authMode === "login" ? "Masuk " : "Daftar " ;
        byId("auth-form").elements.password.autocomplete = state.authMode === "login" ? "current-password" : "new-password";
    }));

    byId("auth-form").addEventListener("submit", async event => {
        event.preventDefault();
        const form = new FormData(event.currentTarget);
        try {
            const user = await api(`/api/auth/${state.authMode}`, { method: "POST", body: JSON.stringify({ email: form.get("email"), password: form.get("password") }) });
            await showWorkspace(user);
        } catch (error) {
            setMessage(byId("auth-message"), error.message, true);
        }
    });

    byId("bootstrap-form").addEventListener("submit", async event => {
        event.preventDefault();
        const form = new FormData(event.currentTarget);
        try {
            const user = await api("/api/auth/bootstrap-admin", { method: "POST", body: JSON.stringify({ email: form.get("email"), password: form.get("password"), bootstrapKey: form.get("bootstrapKey") }) });
            await showWorkspace(user);
        } catch (error) {
            setMessage(byId("auth-message"), error.message, true);
        }
    });

    byId("logout-button").addEventListener("click", async () => {
        await api("/api/auth/logout", { method: "POST" });
        window.location.reload();
    });

    byId("checkout-button").addEventListener("click", async () => {
        try {
            const order = await api("/api/orders", {
                method: "POST",
                body: JSON.stringify({ paymentMethod: byId("payment-method").value, items: [...state.cart].map(([productId, line]) => ({ productId, quantity: line.quantity })) })
            });
            state.cart.clear();
            renderCart();
            setMessage(byId("cart-message"), `Pesanan ${order.orderNumber} berhasil dibuat.`);
            await loadCatalog();
            await loadCustomerOrders();
        } catch (error) {
            setMessage(byId("cart-message"), error.message, true);
        }
    });

    byId("product-form").addEventListener("submit", async event => {
        event.preventDefault();
        const form = event.currentTarget;
        const data = new FormData(form);
        const id = data.get("id");
        const resource = Object.fromEntries(["name", "quantityInPackage", "unitOfMeasurement", "price", "stockQuantity", "categoryId"].map(key => [key, key === "name" ? data.get(key) : Number(data.get(key))]));
        try {
            await api(id ? `/api/products/${id}` : "/api/products", { method: id ? "PUT" : "POST", body: JSON.stringify(resource) });
            form.reset();
            form.elements.id.value = "";
            byId("product-form-title").textContent = "Tambah produk";
            await loadCatalog();
            setMessage(byId("product-message"), "Produk tersimpan.");
        } catch (error) {
            setMessage(byId("product-message"), error.message, true);
        }
    });

    byId("reset-product").addEventListener("click", () => {
        byId("product-form").reset();
        byId("product-form").elements.id.value = "";
        byId("product-form-title").textContent = "Tambah produk";
    });

    byId("category-form").addEventListener("submit", async event => {
        event.preventDefault();
        try {
            await api("/api/categories", { method: "POST", body: JSON.stringify({ name: new FormData(event.currentTarget).get("name") }) });
            event.currentTarget.reset();
            await loadCatalog();
            setMessage(byId("app-message"), "Kategori ditambahkan.");
        } catch (error) {
            setMessage(byId("app-message"), error.message, true);
        }
    });

    byId("run-reports").addEventListener("click", () => loadReports().catch(error => setMessage(byId("app-message"), error.message, true)));
    document.querySelectorAll(".refresh-button").forEach(item => item.addEventListener("click", () => loadStaff().catch(error => setMessage(byId("app-message"), error.message, true))));
    document.querySelectorAll("[data-panel]").forEach(tab => tab.addEventListener("click", () => {
        document.querySelectorAll("[data-panel]").forEach(item => item.classList.toggle("selected", item === tab));
        document.querySelectorAll(".work-panel").forEach(panel => panel.hidden = panel.id !== tab.dataset.panel);
        if (tab.dataset.panel === "reports-panel") loadReports().catch(error => setMessage(byId("app-message"), error.message, true));
    }));

    byId("report-from").value = monthAgo;
    byId("report-to").value = today;
    Promise.all([loadCatalog(), refreshUser()]).catch(error => {
        setConnection(false);
        setMessage(byId("auth-message"), error.message, true);
    });
})();