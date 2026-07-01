document.addEventListener('DOMContentLoaded', function () {

    // ── Parallax quả cầu sáng theo chuột trong hero ─────────────────────
    const hero = document.querySelector('.hero-section');
    const orbs = document.querySelectorAll('.hero-orb');
    if (hero && orbs.length) {
        hero.addEventListener('mousemove', (e) => {
            const rect = hero.getBoundingClientRect();
            const px = (e.clientX - rect.left) / rect.width - 0.5;
            const py = (e.clientY - rect.top) / rect.height - 0.5;
            orbs.forEach((orb, i) => {
                const strength = (i + 1) * 10;
                orb.style.transform = `translate(${px * strength}px, ${py * strength}px)`;
            });
        });
        hero.addEventListener('mouseleave', () => {
            orbs.forEach(orb => orb.style.transform = 'translate(0,0)');
        });
    }

    // ── Đếm số tăng dần cho hero-stats ──────────────────────────────────
    function animateCount(el) {
        const raw = el.textContent.trim();
        const match = raw.match(/^(\d+)(.*)$/); // tách số và hậu tố (VD: "128+" → 128, "+")
        if (!match) return; // không phải dạng số (VD: "100%", "24/7") → bỏ qua
        const target = parseInt(match[1], 10);
        const suffix = match[2] || '';
        const duration = 900;
        const start = performance.now();

        function tick(now) {
            const progress = Math.min((now - start) / duration, 1);
            const eased = 1 - Math.pow(1 - progress, 3);
            const value = Math.floor(eased * target);
            el.textContent = value + suffix;
            if (progress < 1) requestAnimationFrame(tick);
            else {
                el.textContent = target + suffix;
                el.classList.add('counted');
            }
        }
        requestAnimationFrame(tick);
    }

    document.querySelectorAll('.hstat-num').forEach(animateCount);

    // ── Ticker: nhân đôi item để cuộn liền mạch vô hạn ──────────────────
    document.querySelectorAll('.home-ticker-track').forEach(track => {
        const original = track.innerHTML;
        track.innerHTML = original + original; // gấp đôi cho vòng lặp mượt
    });

});