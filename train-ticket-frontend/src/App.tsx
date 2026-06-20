import { Route, Routes } from "react-router-dom";
import { useLocation } from "react-router-dom";
import Navbar from "./components/Navbar";
import AdminBookingsPage from "./pages/admin/AdminBookingsPage";
import AdminCreateRoutePage from "./pages/admin/AdminCreateRoutePage";
import AdminCreateSchedulePage from "./pages/admin/AdminCreateSchedulePage";
import AdminCreateTrainPage from "./pages/admin/AdminCreateTrainPage";
import AdminDashboardPage from "./pages/admin/AdminDashboardPage";
import AdminDiscountsPage from "./pages/admin/AdminDiscountsPage";
import AdminPricingPage from "./pages/admin/AdminPricingPage";
import AdminRevenuePage from "./pages/admin/AdminRevenuePage";
import AdminRoutesPage from "./pages/admin/AdminRoutesPage";
import AdminSchedulesPage from "./pages/admin/AdminSchedulesPage";
import AdminTrainsPage from "./pages/admin/AdminTrainsPage";
import AdminUsersPage from "./pages/admin/AdminUsersPage";
import BookingCheckoutPage from "./pages/BookingCheckoutPage";
import DataRequestPage from "./pages/DataRequestPage";
import HomePage from "./pages/HomePage";
import LoginPage from "./pages/LoginPage";
import MyBookingsPage from "./pages/MyBookingsPage";
import MyProfilePage from "./pages/MyProfilePage";
import OrderSummaryPage from "./pages/OrderSummaryPage";
import RegisterPage from "./pages/RegisterPage";
import SearchResultsPage from "./pages/SearchResultsPage";
import SeatMapPage from "./pages/SeatMapPage";
import SummaryPage from "./pages/SummaryPage";
import "./App.css";

function App() {
  const location = useLocation();
  const isAdminRoute = location.pathname.startsWith("/admin");

  return (
    <div className="app-shell">
      {!isAdminRoute && <Navbar />}
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/admin" element={<AdminDashboardPage />} />
        <Route path="/admin/trains" element={<AdminTrainsPage />} />
        <Route path="/admin/trains/new" element={<AdminCreateTrainPage />} />
        <Route path="/admin/routes" element={<AdminRoutesPage />} />
        <Route path="/admin/routes/new" element={<AdminCreateRoutePage />} />
        <Route path="/admin/schedules" element={<AdminSchedulesPage />} />
        <Route path="/admin/schedules/new" element={<AdminCreateSchedulePage />} />
        <Route path="/admin/pricing" element={<AdminPricingPage />} />
        <Route path="/admin/bookings" element={<AdminBookingsPage />} />
        <Route path="/admin/users" element={<AdminUsersPage />} />
        <Route path="/admin/discounts" element={<AdminDiscountsPage />} />
        <Route path="/admin/revenue" element={<AdminRevenuePage />} />
        <Route path="/search" element={<SearchResultsPage />} />
        <Route path="/profile" element={<MyProfilePage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/bookings" element={<MyBookingsPage />} />
        <Route path="/seat-map/:tripId" element={<SeatMapPage />} />
        <Route path="/summary/:tripId" element={<SummaryPage />} />
        <Route path="/data/:tripId" element={<DataRequestPage />} />
        <Route path="/order-summary/:tripId" element={<OrderSummaryPage />} />
        <Route path="/checkout/:tripId" element={<BookingCheckoutPage />} />
      </Routes>
    </div>
  );
}

export default App;
