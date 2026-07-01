document.addEventListener('DOMContentLoaded', function () {

    // ── Navbar shadow khi cuộn ──────────────────────────────────
    const wdtNav = document.getElementById('wdtNav');
    function onScrollNav() {
        if (!wdtNav) return;
        if (window.scrollY > 6) wdtNav.classList.add('is-scrolled');
        else wdtNav.classList.remove('is-scrolled');
    }
    window.addEventListener('scroll', onScrollNav, { passive: true });
    onScrollNav();

    // ── Back to top ──────────────────────────────────────────────
    const backToTop = document.getElementById('wdtBackToTop');
    function onScrollTop() {
        if (!backToTop) return;
        if (window.scrollY > 400) backToTop.classList.add('show');
        else backToTop.classList.remove('show');
    }
    window.addEventListener('scroll', onScrollTop, { passive: true });
    backToTop?.addEventListener('click', () => window.scrollTo({ top: 0, behavior: 'smooth' }));

    // ── Mobile drawer ────────────────────────────────────────────
    const burger = document.getElementById('wdtBurger');
    const drawer = document.getElementById('wdtDrawer');
    const overlay = document.getElementById('wdtDrawerOverlay');
    const closeBtn = document.getElementById('wdtDrawerClose');
    const avatarMob = document.getElementById('wdtAvatarMobile');

    function openDrawer() {
        drawer?.classList.add('open');
        overlay?.classList.add('show');
        burger?.setAttribute('aria-expanded', 'true');
        document.body.style.overflow = 'hidden';
    }
    function closeDrawer() {
        drawer?.classList.remove('open');
        overlay?.classList.remove('show');
        burger?.setAttribute('aria-expanded', 'false');
        document.body.style.overflow = '';
    }

    burger?.addEventListener('click', openDrawer);
    avatarMob?.addEventListener('click', openDrawer);
    closeBtn?.addEventListener('click', closeDrawer);
    overlay?.addEventListener('click', closeDrawer);
    document.addEventListener('keydown', e => { if (e.key === 'Escape') closeDrawer(); });

    // ── Ripple nhẹ khi bấm nút / link nút ──────────────────────────
    document.addEventListener('click', function (e) {
        const el = e.target.closest('.btn, .wdt-nav-link, .btn-nav-login, .btn-nav-register');
        if (!el) return;
        const circle = document.createElement('span');
        const rect = el.getBoundingClientRect();
        const size = Math.max(rect.width, rect.height);
        circle.className = 'wdt-ripple';
        circle.style.width = circle.style.height = size + 'px';
        circle.style.left = (e.clientX - rect.left - size / 2) + 'px';
        circle.style.top = (e.clientY - rect.top - size / 2) + 'px';
        el.style.position = el.style.position || 'relative';
        el.style.overflow = 'hidden';
        el.appendChild(circle);
        setTimeout(() => circle.remove(), 600);
    });

});