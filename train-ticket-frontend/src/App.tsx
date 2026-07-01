import { lazy, Suspense } from "react";
import { Route, Routes } from "react-router-dom";
import { useLocation } from "react-router-dom";
import Navbar from "./components/Navbar";
import "./App.css";

const AdminBookingsPage = lazy(() => import("./pages/admin/AdminBookingsPage"));
const AdminAuditLogsPage = lazy(() => import("./pages/admin/AdminAuditLogsPage"));
const AdminCreateRoutePage = lazy(() => import("./pages/admin/AdminCreateRoutePage"));
const AdminCreateSchedulePage = lazy(() => import("./pages/admin/AdminCreateSchedulePage"));
const AdminCreateTrainPage = lazy(() => import("./pages/admin/AdminCreateTrainPage"));
const AdminDashboardPage = lazy(() => import("./pages/admin/AdminDashboardPage"));
const AdminDiscountsPage = lazy(() => import("./pages/admin/AdminDiscountsPage"));
const AdminOpenRailwayImportPage = lazy(() => import("./pages/admin/AdminOpenRailwayImportPage"));
const AdminPricingPage = lazy(() => import("./pages/admin/AdminPricingPage"));
const AdminRevenuePage = lazy(() => import("./pages/admin/AdminRevenuePage"));
const AdminRoutesPage = lazy(() => import("./pages/admin/AdminRoutesPage"));
const AdminSchedulesPage = lazy(() => import("./pages/admin/AdminSchedulesPage"));
const AdminTrainsPage = lazy(() => import("./pages/admin/AdminTrainsPage"));
const AdminUsersPage = lazy(() => import("./pages/admin/AdminUsersPage"));
const BookingCheckoutPage = lazy(() => import("./pages/BookingCheckoutPage"));
const ContactPage = lazy(() => import("./pages/ContactPage"));
const CurrentTripPage = lazy(() => import("./pages/CurrentTripPage"));
const DataRequestPage = lazy(() => import("./pages/DataRequestPage"));
const DiscountSelectionPage = lazy(() => import("./pages/DiscountSelectionPage"));
const DomesticOfferDetailPage = lazy(() => import("./pages/DomesticOfferDetailPage"));
const DomesticOffersPage = lazy(() => import("./pages/DomesticOffersPage"));
const ExplorePolandPage = lazy(() => import("./pages/ExplorePolandPage"));
const FilterSelectionPage = lazy(() => import("./pages/FilterSelectionPage"));
const FaqPage = lazy(() => import("./pages/FaqPage"));
const HelpPage = lazy(() => import("./pages/HelpPage"));
const HomePage = lazy(() => import("./pages/HomePage"));
const LoginPage = lazy(() => import("./pages/LoginPage"));
const MealOfferPage = lazy(() => import("./pages/MealOfferPage"));
const MyBookingsPage = lazy(() => import("./pages/MyBookingsPage"));
const MyProfilePage = lazy(() => import("./pages/MyProfilePage"));
const OrderSummaryPage = lazy(() => import("./pages/OrderSummaryPage"));
const OffersPage = lazy(() => import("./pages/OffersPage"));
const OurTrainsPage = lazy(() => import("./pages/OurTrainsPage"));
const TrainEicPage = lazy(() => import("./pages/TrainEicPage"));
const TrainEipPage = lazy(() => import("./pages/TrainEipPage"));
const TrainIcPage = lazy(() => import("./pages/TrainIcPage"));
const TrainTlkPage = lazy(() => import("./pages/TrainTlkPage"));
const PassengerRightsPage = lazy(() => import("./pages/PassengerRightsPage"));
const RegisterPage = lazy(() => import("./pages/RegisterPage"));
const RefundPolicyPage = lazy(() => import("./pages/RefundPolicyPage"));
const SearchResultsPage = lazy(() => import("./pages/SearchResultsPage"));
const SeatMapPage = lazy(() => import("./pages/SeatMapPage"));
const SleeperOfferPage = lazy(() => import("./pages/SleeperOfferPage"));
const StudentOfferPage = lazy(() => import("./pages/StudentOfferPage"));
const SummaryPage = lazy(() => import("./pages/SummaryPage"));

function App() {
  const location = useLocation();
  const isAdminRoute = location.pathname.startsWith("/admin");

  return (
    <div className="app-shell">
      {!isAdminRoute && <Navbar />}
      <Suspense fallback={<main className="route-loading" aria-live="polite">Loading...</main>}>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/admin" element={<AdminDashboardPage />} />
          <Route path="/admin/trains" element={<AdminTrainsPage />} />
          <Route path="/admin/trains/new" element={<AdminCreateTrainPage />} />
          <Route path="/admin/trains/:trainId/edit" element={<AdminCreateTrainPage />} />
          <Route path="/admin/routes" element={<AdminRoutesPage />} />
          <Route path="/admin/routes/new" element={<AdminCreateRoutePage />} />
          <Route path="/admin/routes/:routeId/edit" element={<AdminCreateRoutePage />} />
          <Route path="/admin/open-railway" element={<AdminOpenRailwayImportPage />} />
          <Route path="/admin/schedules" element={<AdminSchedulesPage />} />
          <Route path="/admin/schedules/new" element={<AdminCreateSchedulePage />} />
          <Route path="/admin/pricing" element={<AdminPricingPage />} />
          <Route path="/admin/bookings" element={<AdminBookingsPage />} />
          <Route path="/admin/users" element={<AdminUsersPage />} />
          <Route path="/admin/discounts" element={<AdminDiscountsPage />} />
          <Route path="/admin/revenue" element={<AdminRevenuePage />} />
          <Route path="/admin/audit-logs" element={<AdminAuditLogsPage />} />
          <Route path="/search" element={<SearchResultsPage />} />
          <Route path="/help" element={<HelpPage />} />
          <Route path="/help/faq" element={<FaqPage />} />
          <Route path="/help/refund-policy" element={<RefundPolicyPage />} />
          <Route path="/help/passenger-rights" element={<PassengerRightsPage />} />
          <Route path="/contact" element={<ContactPage />} />
          <Route path="/profile" element={<MyProfilePage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/discounts" element={<DiscountSelectionPage />} />
          <Route path="/filters" element={<FilterSelectionPage />} />
          <Route path="/offers" element={<OffersPage />} />
          <Route path="/offers/domestic" element={<DomesticOffersPage />} />
          <Route path="/offers/domestic/:offerId" element={<DomesticOfferDetailPage />} />
          <Route path="/offers/explore" element={<ExplorePolandPage />} />
          <Route path="/offers/meal" element={<MealOfferPage />} />
          <Route path="/offers/sleeper" element={<SleeperOfferPage />} />
          <Route path="/offers/student" element={<StudentOfferPage />} />
          <Route path="/trains" element={<OurTrainsPage />} />
          <Route path="/trains/eic" element={<TrainEicPage />} />
          <Route path="/trains/eip" element={<TrainEipPage />} />
          <Route path="/trains/ic" element={<TrainIcPage />} />
          <Route path="/trains/tlk" element={<TrainTlkPage />} />
          <Route path="/bookings" element={<MyBookingsPage />} />
          <Route path="/seat-map/:tripId" element={<SeatMapPage />} />
          <Route path="/summary/:tripId" element={<SummaryPage />} />
          <Route path="/data/:tripId" element={<DataRequestPage />} />
          <Route path="/order-summary/:tripId" element={<OrderSummaryPage />} />
          <Route path="/checkout/:tripId" element={<BookingCheckoutPage />} />
          <Route path="/trip/:bookingId" element={<CurrentTripPage />} />
        </Routes>
      </Suspense>
    </div>
  );
}

export default App;
