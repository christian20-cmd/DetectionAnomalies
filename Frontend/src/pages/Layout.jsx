
import Sidebar from '../components/common/Sidebar'
import Dashboard from './Dashboard'

const Layout = () => {
  const handleLogout = () => {
    console.log('Déconnexion')
  }

  return (
    <div className="dark:bg-black">
      <div className="flex h-screen overflow-hidden bg-black/10 dark:bg-white/5 transition-colors duration-300">
        <Sidebar onLogout={handleLogout} />
        <main className="flex-1 overflow-auto p-4">
          <Dashboard />
        </main>
      </div>
    </div>
  )
}

export default Layout