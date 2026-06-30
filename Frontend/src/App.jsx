import { Routes, Route, Navigate } from 'react-router-dom'
import Connexion from './pages/auth/Connexion'

import Dashboard from './pages/Dashboard'


export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/connexion" replace />} />
      <Route path="/connexion" element={<Connexion />} />
      <Route path="/dashboard" element={<Dashboard />} />
    </Routes>
  )
}