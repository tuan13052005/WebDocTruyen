document.addEventListener('DOMContentLoaded', function () {

    // ── Scroll-reveal: card/list item xuất hiện khi cuộn tới ──────────
    const revealTargets = document.querySelectorAll('.reveal-card, .chap-slide-in');
    if (revealTargets.length) {
        const io = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('is-visible');
                    io.unobserve(entry.target);
                }
            });
        }, { threshold: 0.12, rootMargin: '0px 0px -40px 0px' });

        revealTargets.forEach((el, i) => {
            el.style.transitionDelay = Math.min(i * 40, 400) + 'ms';
            io.observe(el);
        });
    }

    // ── Tilt effect theo chuột cho card truyện ─────────────────────────
    document.querySelectorAll('.tilt-card').forEach(card => {
        card.addEventListener('mousemove', e => {
            const rect = card.getBoundingClientRect();
            const px = (e.clientX - rect.left) / rect.width - 0.5;
            const py = (e.clientY - rect.top) / rect.height - 0.5;
            card.style.setProperty('--ry', (px * 8).toFixed(2) + 'deg');
            card.style.setProperty('--rx', (-py * 8).toFixed(2) + 'deg');
        });
        card.addEventListener('mouseleave', () => {
            card.style.setProperty('--rx', '0deg');
            card.style.setProperty('--ry', '0deg');
        });
    });

    // ── Hiệu ứng tim nổ khi bấm nút Theo dõi (favorite) ─────────────────
    function burstHeart(btn) {
        const colors = ['#f43f5e', '#ec4899', '#fb7185'];
        for (let i = 0; i < 8; i++) {
            const p = document.createElement('span');
            p.className = 'fav-particle';
            const angle = (Math.PI * 2 * i) / 8;
            const dist = 22 + Math.random() * 10;
            p.style.setProperty('--dx', (Math.cos(angle) * dist) + 'px');
            p.style.setProperty('--dy', (Math.sin(angle) * dist) + 'px');
            p.style.background = colors[i % colors.length];
            btn.appendChild(p);
            setTimeout(() => p.remove(), 650);
        }
    }

    document.querySelectorAll('#favBtn, [id^="favBtn_"]').forEach(btn => {
        btn.classList.add('fav-burst-wrap');
        btn.addEventListener('click', () => {
            // Chỉ nổ pháo khi sắp chuyển sang trạng thái "đang theo dõi"
            if (!btn.classList.contains('is-fav')) {
                setTimeout(() => burstHeart(btn), 30);
            }
        });
    });

    // ── Sao rating: hiệu ứng "pop" khi click ────────────────────────────
    document.querySelectorAll('.sd-my-star').forEach(star => {
        star.addEventListener('click', () => {
            star.classList.remove('star-pop');
            void star.offsetWidth; // reflow để restart animation
            star.classList.add('star-pop');
        });
    });

    // ── Ảnh bìa: bỏ shimmer khi ảnh đã tải xong ─────────────────────────
    document.querySelectorAll('img.img-shimmer').forEach(img => {
        if (img.complete) {
            img.classList.remove('img-shimmer');
        } else {
            img.addEventListener('load', () => img.classList.remove('img-shimmer'), { once: true });
            img.addEventListener('error', () => img.classList.remove('img-shimmer'), { once: true });
        }
    });

});