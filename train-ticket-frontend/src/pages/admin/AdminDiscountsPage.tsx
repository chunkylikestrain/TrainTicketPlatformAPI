import { DeleteOutlined, EditOutlined, GiftOutlined, PlusOutlined } from "@ant-design/icons";
import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { createAdminDiscount, deleteAdminDiscount, getAdminDiscounts, updateAdminDiscount } from "../../api/adminApi";
import AdminLayout from "../../components/AdminLayout";
import type { AdminDiscount } from "../../types/admin";

type DiscountClass = "Class 2 only" | "Class 1 and 2";
type DiscountStatus = "Active" | "Draft";

function AdminDiscountsPage() {
  const [discounts, setDiscounts] = useState<AdminDiscount[]>([]);
  const [editingDiscount, setEditingDiscount] = useState<AdminDiscount | null>(null);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [name, setName] = useState("");
  const [percent, setPercent] = useState("37");
  const [eligibleClass, setEligibleClass] = useState<DiscountClass>("Class 2 only");
  const [documentHint, setDocumentHint] = useState("Document checked by ticket inspector");
  const [status, setStatus] = useState<DiscountStatus>("Active");

  useEffect(() => {
    getAdminDiscounts().then(setDiscounts);
  }, []);

  function openForm(discount?: AdminDiscount) {
    setEditingDiscount(discount ?? null);
    setName(discount?.name ?? "");
    setPercent(String(discount?.percent ?? 37));
    setEligibleClass((discount?.eligibleClass ?? "Class 2 only") as DiscountClass);
    setDocumentHint(discount?.documentHint ?? "Document checked by ticket inspector");
    setStatus((discount?.status ?? "Active") as DiscountStatus);
    setIsFormOpen(true);
  }

  function closeForm() {
    setIsFormOpen(false);
    setEditingDiscount(null);
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const nextDiscount = {
      id: editingDiscount?.id ?? Date.now(),
      name,
      percent: Number(percent),
      eligibleClass,
      documentHint,
      status,
    };

    const saved = editingDiscount
      ? await updateAdminDiscount(nextDiscount)
      : await createAdminDiscount(nextDiscount);

    setDiscounts((current) => editingDiscount
      ? current.map((discount) => (discount.id === editingDiscount.id ? saved : discount))
      : [...current, saved]);
    closeForm();
  }

  async function handleDelete(id: number) {
    await deleteAdminDiscount(id);
    setDiscounts((current) => current.filter((discount) => discount.id !== id));
  }

  return (
    <AdminLayout>
      <section className="admin-page-heading">
        <div>
          <h1><GiftOutlined /> Manage Discounts</h1>
          <p>Configure percentage discounts. Riders provide documents to the ticket inspector during travel.</p>
        </div>
        <button type="button" className="admin-primary-button" onClick={() => openForm()}>
          <PlusOutlined /> Add discount
        </button>
      </section>

      <section className="admin-stat-grid pricing-stat-grid">
        <article className="admin-stat-card"><span className="stat-orange"><GiftOutlined /></span><div><small>Discount rules</small><strong>{discounts.length}</strong></div></article>
        <article className="admin-stat-card"><span className="stat-blue"><GiftOutlined /></span><div><small>Class 2 only</small><strong>{discounts.filter((discount) => discount.eligibleClass === "Class 2 only").length}</strong></div></article>
        <article className="admin-stat-card"><span className="stat-purple"><GiftOutlined /></span><div><small>Class 1 allowed</small><strong>{discounts.filter((discount) => discount.eligibleClass === "Class 1 and 2").length}</strong></div></article>
      </section>

      {isFormOpen && (
        <form className="admin-editor-panel" onSubmit={handleSubmit}>
          <h2>{editingDiscount ? "Edit discount" : "Add discount"}</h2>
          <label>Discount name<input value={name} onChange={(event) => setName(event.target.value)} placeholder="Example: Student" required /></label>
          <label>Discount percent<input value={percent} onChange={(event) => setPercent(event.target.value)} min="0" max="100" type="number" required /></label>
          <label>Eligible class<select value={eligibleClass} onChange={(event) => setEligibleClass(event.target.value as DiscountClass)}>
            <option>Class 2 only</option>
            <option>Class 1 and 2</option>
          </select></label>
          <label>Status<select value={status} onChange={(event) => setStatus(event.target.value as DiscountStatus)}>
            <option>Active</option>
            <option>Draft</option>
          </select></label>
          <label className="admin-editor-wide">Inspector document note<input value={documentHint} onChange={(event) => setDocumentHint(event.target.value)} required /></label>
          <div className="admin-form-actions">
            <button type="submit" className="admin-primary-button">Save discount</button>
            <button type="button" className="admin-secondary-button" onClick={closeForm}>Cancel</button>
          </div>
        </form>
      )}

      <section className="admin-table-card">
        <table className="admin-table">
          <thead>
            <tr><th>Discount</th><th>Percent</th><th>Eligible class</th><th>Inspector check</th><th>Status</th><th>Actions</th></tr>
          </thead>
          <tbody>
            {discounts.map((discount) => (
              <tr key={discount.id}>
                <td><strong>{discount.name}</strong></td>
                <td>{discount.percent}%</td>
                <td>{discount.eligibleClass}</td>
                <td>{discount.documentHint}</td>
                <td><span className={discount.status === "Active" ? "status-pill status-active" : "status-pill status-warning"}>{discount.status}</span></td>
                <td>
                  <button type="button" onClick={() => openForm(discount)} aria-label={`Edit ${discount.name}`}><EditOutlined /></button>
                  <button type="button" onClick={() => handleDelete(discount.id)} aria-label={`Delete ${discount.name}`}><DeleteOutlined /></button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </AdminLayout>
  );
}

export default AdminDiscountsPage;
