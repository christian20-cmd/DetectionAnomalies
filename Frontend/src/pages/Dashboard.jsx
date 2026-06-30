
import Sidebar from '../components/common/Sidebar'


const Dashboard = () => {
  const handleLogout = () => {
    console.log('Déconnexion')
  }

  return (
    <div className="dark:bg-black">
      <div className="flex h-screen overflow-hidden bg-black/10 dark:bg-white/5 transition-colors duration-300">
        <Sidebar onLogout={handleLogout} />
        <main className="flex-1 overflow-auto p-4">
            <h1
                className="text-xl italic font-bold text-black dark:text-white"
                style={{ fontFamily: "'Playfair Display', Georgia, sans-serif", minHeight: '2.5rem' }}
                >
                    Dashboard
            </h1>
        </main>
      </div>
    </div>
  )
}

export default Dashboard