(() => {
  'use strict';

  // ---------- ظهور العناصر عند التمرير ----------
  const io = new IntersectionObserver((entries) => {
    entries.forEach(e => {
      if (e.isIntersecting) { e.target.classList.add('is-visible'); io.unobserve(e.target); }
    });
  }, { threshold: 0.12 });
  document.querySelectorAll('.reveal').forEach(el => io.observe(el));

  // ---------- عدّاد أرقام متحرك ----------
  function animateNumber(el, to, duration = 700) {
    const from = parseInt(el.textContent, 10) || 0;
    if (from === to) return;
    const start = performance.now();
    const step = (now) => {
      const t = Math.min((now - start) / duration, 1);
      const eased = 1 - Math.pow(1 - t, 3);
      el.textContent = Math.round(from + (to - from) * eased);
      if (t < 1) requestAnimationFrame(step);
    };
    requestAnimationFrame(step);
    el.classList.remove('flash');
    void el.offsetWidth;           // لإعادة تشغيل الأنيميشن
    el.classList.add('flash');
  }
  window.animateNumber = animateNumber;

  // الأرقام اللي عليها count-up بتعدّ من 0 لقيمتها عند فتح الصفحة
  document.querySelectorAll('.count-up').forEach(el => {
    const target = parseInt(el.dataset.value ?? el.textContent, 10) || 0;
    el.textContent = '0';
    animateNumber(el, target, 1000);
  });

  // ---------- Toast ----------
  function showToast(message, type = 'success') {
    const area = document.getElementById('toastArea');
    if (!area || !window.bootstrap) return;
    const el = document.createElement('div');
    el.className = `toast align-items-center text-bg-${type} border-0`;
    el.setAttribute('role', 'alert');
    el.innerHTML = '<div class="d-flex"><div class="toast-body"></div>' +
      '<button type="button" class="btn-close btn-close-white m-auto me-2" data-bs-dismiss="toast"></button></div>';
    el.querySelector('.toast-body').textContent = message;   // textContent لتجنب XSS
    area.appendChild(el);
    el.addEventListener('hidden.bs.toast', () => el.remove());
    new bootstrap.Toast(el, { delay: 4000 }).show();
  }
  window.showToast = showToast;

  // رسائل TempData القادمة من السيرفر
  const area = document.getElementById('toastArea');
  if (area) {
    if (area.dataset.success) showToast(area.dataset.success, 'success');
    if (area.dataset.error) showToast(area.dataset.error, 'danger');
  }

  // ---------- تأثير الموجة على الأزرار ----------
  document.addEventListener('click', (e) => {
    const btn = e.target.closest('.btn');
    if (!btn) return;
    const r = btn.getBoundingClientRect();
    const size = Math.max(r.width, r.height);
    const s = document.createElement('span');
    s.className = 'ripple';
    s.style.width = s.style.height = size + 'px';
    s.style.left = (e.clientX - r.left - size / 2) + 'px';
    s.style.top = (e.clientY - r.top - size / 2) + 'px';
    btn.appendChild(s);
    setTimeout(() => s.remove(), 600);
  });

  // ---------- منع الضغط المزدوج على الإرسال + spinner ----------
  document.addEventListener('submit', (e) => {
    if (e.defaultPrevented) return;                 // الـvalidation رفض الفورم
    const btn = e.target.querySelector('[type=submit]');
    if (!btn || btn.dataset.loading) return;
    btn.dataset.loading = '1';
    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>' + btn.textContent;
    setTimeout(() => (btn.disabled = true), 0);
  });

  // ---------- إظهار/إخفاء كلمة المرور ----------
  document.querySelectorAll('[data-toggle-password]').forEach(btn => {
    btn.addEventListener('click', () => {
      const input = btn.parentElement.querySelector('input');
      const show = input.type === 'password';
      input.type = show ? 'text' : 'password';
      btn.innerHTML = show ? '<i class="bi bi-eye-slash"></i>' : '<i class="bi bi-eye"></i>';
    });
  });

  // ---------- بطاقة الدور: تحديث تلقائي كل 10 ثواني ----------
  const card = document.getElementById('queueCard');
  if (card) {
    const url = card.dataset.statusUrl;
    const near = parseInt(card.dataset.near, 10) || 2;
    const $ = (id) => document.getElementById(id);
    let notified = false;

    function updateNumbers(d) {
      [['currentNum', d.currentNumber], ['yourNum', d.yourNumber], ['beforeNum', d.patientsBefore]]
        .forEach(([id, v]) => { animateNumber($(id), v); $(id).dataset.value = v; });
    }

       function updateState(d, silent) {
      const p = d.yourNumber > 0 ? Math.min(100, Math.round(d.currentNumber * 100 / d.yourNumber)) : 0;
      $('ringFg').style.setProperty('--progress', p);
      card.dataset.yourTurn = d.isYourTurn;

      let state = 'wait', text = 'تتحدث هذه الصفحة تلقائيا دون الحاجة الى إعادة التحميل';
      if (d.yourNumber === 0) { state = 'none'; text = ' لا يوجد لديك دور فعّال حالياً'; }
      else if (d.isYourTurn) { state = 'now'; text = ' حان دورك الان! تفضل بالتوجه الى العيادة'; }
      else if (d.patientsBefore <= near) {
        state = 'near';
        text = d.patientsBefore === 0 ? 'اقترب دورك، يُرجى الاستعداد' : ' أنت التالي، يُرجى الاستعداد';
      }

      card.dataset.state = state;
      $('queueMsg').textContent = text;

      // زر الإلغاء بيختفي لما الطبيب ينادي عليك
      const cb = document.getElementById('cancelBtn');
      if (cb) cb.classList.toggle('d-none', !d.canCancel);

      const close = state === 'now' || state === 'near';
      if (close && !notified && !silent) {
        showToast(text, state === 'now' ? 'success' : 'warning');
        if (navigator.vibrate) navigator.vibrate(200);
      }
      if (close) notified = true;
    }

    const read = () => ({
      currentNumber: +$('currentNum').dataset.value,
      yourNumber: +$('yourNum').dataset.value,
      patientsBefore: +$('beforeNum').dataset.value,
      isYourTurn: card.dataset.yourTurn === 'true',
      canCancel: true
    });
    updateState(read(), true);   // الحالة الأولية بدون إشعار

    async function poll() {
      if (document.hidden || !url) return;
      try {
        const res = await fetch(url, { headers: { 'Accept': 'application/json' }, cache: 'no-store' });
        if (!res.ok) return;
        const d = await res.json();
        updateNumbers(d);
        updateState(d, false);
      } catch { /* تجاهل أخطاء الشبكة المؤقتة */ }
    }
    setInterval(poll, 10000);
    document.addEventListener('visibilitychange', () => { if (!document.hidden) poll(); });
  }
    // يمنع التعبئة التلقائية: الحقل للقراءة فقط لحد ما المستخدم يضغط عليه
  document.querySelectorAll('input[data-no-autofill]').forEach(input => {
    input.setAttribute('readonly', '');
    input.addEventListener('focus', () => input.removeAttribute('readonly'), { once: true });
  });

  // إذا رجع المستخدم للصفحة بزر "رجوع" بتتفضى الفورم
  window.addEventListener('pageshow', (e) => {
    if (e.persisted) document.querySelectorAll('form').forEach(f => f.reset());
  });
    // حقول الأسماء: أحرف ومسافات فقط
  document.querySelectorAll('input[data-letters-only]').forEach(input => {
    input.addEventListener('input', () => {
      input.value = input.value.replace(/[^A-Za-z\u0600-\u06FF\s]/g, '');
    });
  });
    // تأكيد قبل الإجراءات الحساسة: أي form عليه data-confirm
  document.addEventListener('submit', (e) => {
    const msg = e.target.dataset && e.target.dataset.confirm;
    if (msg && !window.confirm(msg)) e.preventDefault();
  }, true);
})();