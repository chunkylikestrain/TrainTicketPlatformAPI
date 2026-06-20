import AdminLayout from "../../components/AdminLayout";

type AdminPlaceholderPageProps = {
  title: string;
  description: string;
};

function AdminPlaceholderPage({ title, description }: AdminPlaceholderPageProps) {
  return (
    <AdminLayout>
      <section className="admin-page-heading">
        <div>
          <h1>{title}</h1>
          <p>{description}</p>
        </div>
      </section>
      <section className="admin-empty-panel">
        <h2>Ready for the next backend contract</h2>
        <p>This admin section is reserved so navigation and layout are complete before we connect live data.</p>
      </section>
    </AdminLayout>
  );
}

export default AdminPlaceholderPage;
