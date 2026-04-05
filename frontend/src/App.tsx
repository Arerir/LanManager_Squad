import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AppLayout } from './layouts/AppLayout';
import { DashboardPage } from './pages/DashboardPage';
import { EventsPage } from './pages/EventsPage';
import { EventDetailPage } from './pages/EventDetailPage';
import { EventFormPage } from './pages/EventFormPage';
import { UsersPage } from './pages/UsersPage';
import { AttendancePage } from './pages/AttendancePage';

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<AppLayout />}>
          <Route index element={<DashboardPage />} />
          <Route path="events" element={<EventsPage />} />
          <Route path="events/new" element={<EventFormPage />} />
          <Route path="events/:id" element={<EventDetailPage />} />
          <Route path="events/:id/edit" element={<EventFormPage />} />
          <Route path="users" element={<UsersPage />} />
          <Route path="attendance" element={<AttendancePage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
