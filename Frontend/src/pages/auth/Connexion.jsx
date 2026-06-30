import { useState, useEffect, useRef } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Eye, EyeOff, ChevronRight } from 'lucide-react'
import logoW from '../../assets/logoW.png'
import logoB from '../../assets/logoB.png'
const TYPEWRITER_TEXT = 'Connexion'
const TYPEWRITER_SPEED = 110

// ── Credentials ──
const USERS = [
  { email: 'admin@netguard.io', password: 'admin123', role: 'admin' },
  { email: 'user@netguard.io',  password: 'user123',  role: 'user'  },
]

// ── Button skill ──
const BTN_BASE = "flex items-center justify-center w-full gap-2 py-2 text-sm font-medium rounded-full transition disabled:opacity-40 disabled:cursor-not-allowed bg-gray-900 text-white hover:bg-gray-800 dark:bg-gray-200 dark:text-black dark:hover:bg-white"

const Button = ({ children, onClick, type = "button", disabled }) => (
  <button type={type} onClick={onClick} disabled={disabled} className={BTN_BASE}>
    {children}
  </button>
)

// ── Input skill ──
const INP_BASE = "w-full px-6 py-2 text-sm font-medium rounded-xl outline-none transition disabled:opacity-40 disabled:cursor-not-allowed bg-gray-200 text-gray-900  placeholder-gray-400 focus:border-2 focus:border-gray-900  focus:bg-white  dark:bg-white/10 dark:text-white dark:placeholder-white/40 dark:focus:border-white/40 "

const Input = ({ placeholder, value, onChange, type = "text", className = "", ...rest }) => (
  <input
    type={type} value={value} onChange={onChange}
    placeholder={placeholder}
    className={`${INP_BASE} ${className}`}
    {...rest}
  />
)

export default function Connexion() {
  const [email, setEmail]             = useState('')
  const [motDePasse, setMotDePasse]   = useState('')
  const [showMdp, setShowMdp]         = useState(false)
  const [typedTitle, setTypedTitle]   = useState('')
  const [titleDone, setTitleDone]     = useState(false)
  const [pageVisible, setPageVisible] = useState(false)
  const [error, setError]             = useState('')
  const [isDark, setIsDark]           = useState(false)
  const canvasRef = useRef(null)
  const navigate  = useNavigate()

  // Détection dark mode (classe 'dark' sur <html>)
  useEffect(() => {
    const html = document.documentElement
    const check = () => setIsDark(html.classList.contains('dark'))
    check()
    const observer = new MutationObserver(check)
    observer.observe(html, { attributes: true, attributeFilter: ['class'] })
    return () => observer.disconnect()
  }, [])

  // Typewriter
  useEffect(() => {
    let i = 0
    const interval = setInterval(() => {
      i++
      setTypedTitle(TYPEWRITER_TEXT.slice(0, i))
      if (i >= TYPEWRITER_TEXT.length) { clearInterval(interval); setTitleDone(true) }
    }, TYPEWRITER_SPEED)
    return () => clearInterval(interval)
  }, [])

  useEffect(() => {
    const t = setTimeout(() => setPageVisible(true), 30)
    return () => clearTimeout(t)
  }, [])

  useEffect(() => {
    const canvas = canvasRef.current
    if (!canvas) return
    const ctx = canvas.getContext('2d')

    const resize = () => {
      canvas.width  = canvas.offsetWidth  * devicePixelRatio
      canvas.height = canvas.offsetHeight * devicePixelRatio
      ctx.scale(devicePixelRatio, devicePixelRatio)
    }
    resize()
    window.addEventListener('resize', resize)

    const W = () => canvas.offsetWidth
    const H = () => canvas.offsetHeight

    const nodes = Array.from({ length: 30 }, () => ({
      x: Math.random() * 100, y: Math.random() * 100,
      vx: (Math.random() - .5) * .055, vy: (Math.random() - .5) * .055,
      r: Math.random() * 2.4 + 1.1,
      pulse: Math.random() * Math.PI * 2,
      alert: Math.random() < .07
    }))

    const packets = []
    const spawnInterval = setInterval(() => {
      const a = Math.floor(Math.random() * nodes.length)
      let b = Math.floor(Math.random() * nodes.length)
      while (b === a) b = Math.floor(Math.random() * nodes.length)
      const na = nodes[a], nb = nodes[b]
      const dx = nb.x - na.x, dy = nb.y - na.y
      if (Math.sqrt(dx * dx + dy * dy) < 35)
        packets.push({ ax: na.x, ay: na.y, bx: nb.x, by: nb.y, t: 0, speed: .008 + Math.random() * .006, alert: na.alert })
    }, 700)

    let animId
    const draw = () => {
      ctx.clearRect(0, 0, W(), H())

      nodes.forEach(n => {
        n.x += n.vx; n.y += n.vy; n.pulse += .025
        if (n.x < 2 || n.x > 98) n.vx *= -1
        if (n.y < 2 || n.y > 98) n.vy *= -1
      })

      // ── Lignes entre nodes ──
      for (let a = 0; a < nodes.length; a++)
        for (let b = a + 1; b < nodes.length; b++) {
          const na = nodes[a], nb = nodes[b]
          const dx = nb.x - na.x, dy = nb.y - na.y
          const d = Math.sqrt(dx * dx + dy * dy)
          if (d < 28) {
            ctx.beginPath()
            ctx.moveTo(na.x / 100 * W(), na.y / 100 * H())
            ctx.lineTo(nb.x / 100 * W(), nb.y / 100 * H())
            ctx.strokeStyle = isDark
              ? `rgba(255,255,255,${(1 - d / 28) * .16})`
              : `rgba(100,100,100,${(1 - d / 28) * .25})`
            ctx.lineWidth = .7; ctx.stroke()
          }
        }

      // ── Paquets ──
      for (let p = packets.length - 1; p >= 0; p--) {
        const pk = packets[p]; pk.t += pk.speed
        if (pk.t >= 1) { packets.splice(p, 1); continue }
        ctx.beginPath()
        ctx.arc(
          (pk.ax + (pk.bx - pk.ax) * pk.t) / 100 * W(),
          (pk.ay + (pk.by - pk.ay) * pk.t) / 100 * H(),
          2.1, 0, Math.PI * 2
        )
        ctx.fillStyle = pk.alert
          ? 'rgba(239,68,68,.9)'
          : isDark ? 'rgba(255,255,255,.7)' : 'rgba(80,80,80,.7)'
        ctx.fill()
      }

      // ── Nodes ──
      nodes.forEach(n => {
        const px = n.x / 100 * W(), py = n.y / 100 * H(), g = (Math.sin(n.pulse) + 1) / 2
        if (n.alert) {
          ctx.beginPath(); ctx.arc(px, py, n.r + 4 + g * 3, 0, Math.PI * 2)
          ctx.strokeStyle = `rgba(239,68,68,${.15 + g * .2})`; ctx.lineWidth = 1; ctx.stroke()
          ctx.beginPath(); ctx.arc(px, py, n.r, 0, Math.PI * 2)
          ctx.fillStyle = '#ef4444'; ctx.fill()
        } else {
          ctx.beginPath(); ctx.arc(px, py, n.r + 2 + g * 2, 0, Math.PI * 2)
          ctx.fillStyle = isDark
            ? `rgba(255,255,255,${.04 + g * .04})`
            : `rgba(100,100,100,${.06 + g * .05})`
          ctx.fill()
          ctx.beginPath(); ctx.arc(px, py, n.r, 0, Math.PI * 2)
          ctx.fillStyle = isDark
            ? `rgba(255,255,255,${.45 + g * .4})`
            : `rgba(80,80,80,${.5 + g * .4})`
          ctx.fill()
        }
      })

      animId = requestAnimationFrame(draw)
    }
    draw()

    return () => {
      window.removeEventListener('resize', resize)
      clearInterval(spawnInterval)
      cancelAnimationFrame(animId)
    }
  }, [isDark])

  const anim = (delay = 0) => ({
    opacity: pageVisible ? 1 : 0,
    transform: pageVisible ? 'translateY(0)' : 'translateY(16px)',
    transition: `opacity 0.5s ease ${delay}s, transform 0.5s ease ${delay}s`,
  })

  const handleSubmit = (e) => {
    e.preventDefault()
    setError('')
    const match = USERS.find(
      u => u.email === email.trim().toLowerCase() && u.password === motDePasse
    )
    if (match) {
      // Optionnel : stocker l'utilisateur en session
      sessionStorage.setItem('user', JSON.stringify({ email: match.email, role: match.role }))
      navigate('/dashboard')
    } else {
      setError('Email ou mot de passe incorrect.')
    }
  }

  return (
    <div className="flex items-center justify-center min-h-screen bg-white/10 dark:bg-black overflow-hidden pt-20 relative">
      {/* Canvas plein écran */}
      <canvas ref={canvasRef} className="absolute inset-0 w-full h-full" />

      {/* Logo en haut de page (hors carte) */}
      <div
        className="absolute top-8 z-20"
        style={anim(0)}
      >
        <img
          src={isDark ? logoW : logoB}
          alt="NetGuard logo"
          className="h-40 w-auto object-contain"
        />
      </div>

      {/* Carte formulaire centrée */}
      <div
        className="relative z-10 w-full max-w-sm rounded-3xl px-10 py-10 backdrop-blur-sm"
        style={anim(0.05)}
      >
        {/* Titre typewriter */}
        <div className="mb-7" style={anim(0.1)}>
          <h1 className="text-3xl italic font-bold text-gray-900 dark:text-white" style={{ fontFamily: "'Playfair Display', Georgia, serif", minHeight: '2.5rem' }}>
            {typedTitle}
            {!titleDone && (
              <span
                className="inline-block w-[2px] h-7 bg-white ml-0.5 align-middle"
                style={{ animation: 'blink .75s step-end infinite' }}
              />
            )}
          </h1>
          <p className="text-xs text-gray-400 dark:text-white/40 mt-1" style={{ opacity: titleDone ? 1 : 0, transition: 'opacity .4s' }}>
            Accédez à votre espace de surveillance.
          </p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div style={anim(0.28)}>
            <label className="block text-xs font-bold dark:text-white/60 mb-1.5">Adresse email</label>
            <Input
              type="email"
              value={email}
              onChange={e => { setEmail(e.target.value); setError('') }}
              placeholder="Entrez votre email"
            />
          </div>

          <div style={anim(0.34)}>
            <div className="flex justify-between mb-1.5">
              <label className="text-xs font-bold dark:text-white/60">Mot de passe</label>
              <a href="#" className="text-[11px] dark:text-white/35 hover:text-black/75 transition">
                Mot de passe oublié ?
              </a>
            </div>
            <div className="relative">
              <Input
                type={showMdp ? 'text' : 'password'}
                value={motDePasse}
                onChange={e => { setMotDePasse(e.target.value); setError('') }}
                placeholder="••••••••"
                className="pr-10"
              />
              <button
                type="button"
                onClick={() => setShowMdp(!showMdp)}
                className="absolute right-3 top-1/2 -translate-y-1/2 dark:text-white/35 hover:text-black/75 transition"
              >
                {showMdp ? <EyeOff size={15} /> : <Eye size={15} />}
              </button>
            </div>
          </div>
          {/* Message d'erreur */}
          {error && (
            <p className="text-xs text-red-400 text-center -mt-1" style={{ animation: 'fadeIn .2s ease' }}>
              {error}
            </p>
          )}

          <div style={anim(0.4)}>
            <Button type="submit">
              Se connecter <ChevronRight size={14} />
            </Button>
          </div>
        </form>

        <div
          className="mt-6 pt-5 border-t-4 border-black/20 dark:border-white/10 text-center text-xs dark:text-white/35"
          style={anim(0.44)}
        >
          Pas encore de compte ?{' '}
          <Link to="/inscription" className="dark:text-white text-black font-bold hover:underline">
            S'inscrire
          </Link>
        </div>
      </div>

      <style>{`
        @keyframes blink   { 0%,100%{opacity:1} 50%{opacity:0} }
        @keyframes fadeIn  { from{opacity:0;transform:translateY(-4px)} to{opacity:1;transform:translateY(0)} }
      `}</style>
    </div>
  )
}