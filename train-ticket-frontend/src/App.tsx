import { Route, Routes } from "react-router-dom";
import Navbar from "./components/Navbar";
import BookingCheckoutPage from "./pages/BookingCheckoutPage";
import DataRequestPage from "./pages/DataRequestPage";
import HomePage from "./pages/HomePage";
import LoginPage from "./pages/LoginPage";
import MyBookingsPage from "./pages/MyBookingsPage";
import OrderSummaryPage from "./pages/OrderSummaryPage";
import RegisterPage from "./pages/RegisterPage";
import SearchResultsPage from "./pages/SearchResultsPage";
import SeatMapPage from "./pages/SeatMapPage";
import SummaryPage from "./pages/SummaryPage";
import "./App.css";

function App() {
  return (
    <div className="app-shell">
      <Navbar />
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/search" element={<SearchResultsPage />} />
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
