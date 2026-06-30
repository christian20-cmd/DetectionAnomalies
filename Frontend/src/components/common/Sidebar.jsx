import { useState, useEffect } from "react";
import { Link, useLocation } from "react-router-dom";
import { useTheme } from "../../context/ThemeContext";
import {
  LayoutDashboard, Activity, AlertTriangle, Network,
  BarChart2, Settings, Bell, LogOut, Sun, Moon,
  User2Icon, ChevronLeft, ChevronRight,
} from "lucide-react";
import logoW from "../../assets/logoW.png";
import logoB from "../../assets/logoB.png"
import { useNavigate } from 'react-router-dom'


const NAV_ITEMS = [
    {
        group: "Principal",
        items: [
            { label: "Tableau de bord", icon: LayoutDashboard, to: "/dashboard" },
            { label: "Trafic réseau",   icon: Activity,        to: "/trafic" },
            { label: "Anomalies",       icon: AlertTriangle,   to: "/anomalies" },
        ],
    },
    {
        group: "Analyse",
        items: [
            { label: "Topologie réseau", icon: Network,   to: "/topologie" },
            { label: "Statistiques",     icon: BarChart2, to: "/statistiques" },
            { label: "Alertes",          icon: Bell,      to: "/alertes" },
        ],
    },
    {
        group: "Système",
            items: [
            { label: "Paramètres", icon: Settings, to: "/parametres" },
        ],
    },
];

/* ── Tooltip ── */
const Tooltip = ({ label, visible }) => (
    <div className={`absolute left-full ml-3 top-1/2 -translate-y-1/2 z-50 pointer-events-none transition-all duration-150 ${visible ? "opacity-100 translate-x-0" : "opacity-0 -translate-x-1"}`}>
        <div className="bg-gray-900 dark:bg-white text-white dark:text-gray-900 text-[11px] font-medium px-2.5 py-1.5 rounded-lg shadow-lg whitespace-nowrap">
            {label}
            <div className="absolute right-full top-1/2 -translate-y-1/2 border-4 border-transparent border-r-gray-900 dark:border-r-white" />
        </div>
    </div>
);

/* ── Modal de confirmation ── */
const LogoutModal = ({ handleLogout, onCancel }) => (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/10 backdrop-blur-md ">
        <div className="bg-white dark:bg-black rounded-3xl shadow-xl dark:shadow-black/5 p-6 w-80 flex flex-col gap-4">
            <div className="flex flex-col items-center gap-2">
                <div className="w-12 h-12 rounded-full bg-red-100 dark:bg-red-900/30 flex items-center justify-center">
                    <LogOut size={22} className="text-red-600" />
                </div>
                <h2 className="text-sm font-bold text-gray-900 dark:text-white">Se déconnecter ?</h2>
                <p className="text-xs text-gray-500 dark:text-gray-400 text-center">
                    Vous allez quitter votre session. Voulez-vous continuer ?
                </p>
            </div>
            <div className="flex gap-2">
                <button
                    onClick={onCancel}
                    className="flex-1 py-2 rounded-full text-xs font-medium bg-black/10 text-gray-900 dark:bg-white/10 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800 transition">
                    Annuler
                </button>
                <button
                    onClick={handleLogout}
                    className="flex-1 py-2 rounded-full bg-red-600 hover:bg-red-700 text-white text-xs font-semibold transition">
                    Se déconnecter
                </button>
            </div>
        </div>
    </div>
);

const Sidebar = () => {
    const location = useLocation();
    const { theme, toggleTheme } = useTheme();
    const navigate = useNavigate();                    // ← ici, DANS le composantooo
    const [collapsed, setCollapsed] = useState(false);
    const [showLogoutModal, setShowLogoutModal] = useState(false);
    const [tooltip, setTooltip] = useState(null);
    const [isDark, setIsDark]           = useState(false)

    
    // Détection dark mode (classe 'dark' sur <html>)
    useEffect(() => {
        const html = document.documentElement
        const check = () => setIsDark(html.classList.contains('dark'))
        check()
        const observer = new MutationObserver(check)
        observer.observe(html, { attributes: true, attributeFilter: ['class'] })
        return () => observer.disconnect()
    }, [])

    const handleLogout = () => {
        setShowLogoutModal(false);
        navigate('/connexion');
    }
    return (
        <>
            {showLogoutModal && (
                <LogoutModal
                    handleLogout={handleLogout}              // ← passer handleLogout
                    onCancel={() => setShowLogoutModal(false)}
                />
            )}

            <aside className={`flex flex-col h-screen shrink-0 overflow-hidden bg-white dark:bg-black  transition-all duration-300 ${collapsed ? "w-16" : "w-52"}`}>

                {/* ── Header : Logo + Chevron ── */}
                    <div className="flex items-center justify-between px-3 pt-3 pb-2 min-h-[56px]">
                        {!collapsed &&  <img src={isDark ? logoW : logoB} alt="NetGuard logo" className="h-24 w-auto object-contain"/>}
                            
                            {/* Le chevron reste toujours visible, à droite ou centré */}
                            <button
                                onClick={() => setCollapsed(!collapsed)}
                                className={` dark:text-white text-gray-900 flex items-center justify-center transition ${collapsed ? "mx-auto" : "ml-auto"}`}>
                                {collapsed ? <ChevronRight size={40} /> : <ChevronLeft size={40} />}
                            </button>
                        </div>

                    {/* ── Navigation ── */}
                    <nav className="flex-1 overflow-y-auto overflow-x-hidden px-2 py-4">
                        {NAV_ITEMS.map((group) => (
                            <div key={group.group}>
                                <ul className="space-y-1.5 mb-1">
                                    {group.items.map(({ label, icon: Icon, to }) => {
                                        const active = location.pathname === to;
                                        return (
                                            <li key={to} className="relative"
                                                onMouseEnter={() => collapsed && setTooltip(label)}
                                                onMouseLeave={() => setTooltip(null)}>
                                                <Link
                                                    to={to}
                                                    className={`flex items-center gap-4 px-4 py-2 rounded-full text-xs font-medium transition-all duration-150 ${
                                                    collapsed ? "justify-center" : ""
                                                    } ${
                                                    active
                                                        ? "bg-black/10 text-gray-900 dark:bg-white/10 dark:text-white"
                                                        : "text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-white/10 hover:text-black dark:hover:text-white"
                                                    }`}>
                                                    <Icon size={16} className="shrink-0" />
                                                    {!collapsed && label}
                                                </Link>
                                                {collapsed && <Tooltip label={label} visible={tooltip === label} />}
                                            </li>
                                        );
                                    })}
                                </ul>
                            </div>
                        ))}
                    </nav>

                    {/* ── Card utilisateur ── */}
                    <div className={`p-3 mx-2 dark:bg-white/10 bg-black/10 rounded-3xl flex flex-col items-center`}>
                    <div className="flex items-center justify-center mb-1">
                        <span className="text-black p-2 bg-white rounded-full">
                        <User2Icon size={18} />
                        </span>
                    </div>

                    {!collapsed && (
                        <>
                            <p className="text-[10px] text-gray-900 dark:text-gray-400 font-bold text-center leading-relaxed">
                                Administrateur
                            </p>
                            <p className="text-[8px] dark:text-gray-400 text-gray-500 text-center leading-relaxed mb-2.5">
                                christiannomenjanahary4@gmail.com
                            </p>
                        </>
                    )}

                    <button
                        onClick={() => setShowLogoutModal(true)}
                        className={`flex items-center justify-center gap-2 rounded-full bg-red-600 text-white text-xs font-semibold hover:bg-red-700 transition-colors duration-150 ${
                            collapsed ? "w-9 h-9 mt-1" : "w-full py-2"
                        }`}>
                        <LogOut size={14} className="shrink-0" />
                            {!collapsed && "Se déconnecter"}
                        </button>
                    </div>

                    {/* ── Footer Theme ── */}
                    <div className={`flex items-center my-4 px-4 ${collapsed ? "justify-center" : "justify-between gap-3"}`}>

                    {!collapsed && (
                        <div className="flex italic font-bold items-center justify-center gap-1.5"
                            style={{ fontFamily: "'Playfair Display', Georgia, sans-serif" }}>
                            {theme === "dark" ? <Moon size={13} className="text-white" /> : <Sun size={13} className="text-gray-500" />}
                            <span className="text-xs font-medium text-gray-700 dark:text-gray-300">
                                {theme === "dark" ? "Mode sombre" : "Mode clair"}
                            </span>
                        </div>
                    )}

                    {/* Toggle avec tooltip en mode réduit */}
                    <div className="relative"
                        onMouseEnter={() => collapsed && setTooltip("theme")}
                        onMouseLeave={() => setTooltip(null)}>
                            {collapsed ? (
                            <button
                                onClick={toggleTheme}
                                className="w-9 h-9 rounded-full   flex items-center justify-center text-gray-900 dark:text-gray-400 hover:text-black dark:hover:text-white hover:bg-black/10 dark:hover:bg-white/10 active:scale-95 transition">
                                {theme === "dark" ? <Sun size={20} /> : <Moon size={20} />}
                            </button>
                            ) : (
                            <div onClick={toggleTheme}
                                className="relative flex items-center justify-center w-[52px] h-[28px] bg-gray-900 dark:bg-white rounded-full p-1 cursor-pointer">
                                <div className={`absolute w-[22px] h-[22px] bg-white dark:bg-black rounded-full shadow transition-all duration-200 ${theme === "dark" ? "left-[26px]" : "left-[3px]"}`} />
                                    <div className="relative z-10 flex w-full justify-around">
                                    <Sun size={12} className="text-wite dark:text-black" />
                                    <Moon size={12} className="text-white dark:text-white" />
                                </div>
                            </div>
                            )}
                            {collapsed && (
                            <Tooltip
                                label={theme === "dark" ? "Mode clair" : "Mode sombre"}
                                visible={tooltip === "theme"}/>
                        )}
                    </div>

                </div>

            </aside>
        </>
    );
};

export default Sidebar;