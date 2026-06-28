export type Invoice = {
  id: number;
  bookingId: number | null;
  bookingOrderId: number | null;
  invoiceNumber: string;
  buyerName: string;
  buyerEmail: string;
  buyerTaxId: string;
  billingAddress: string;
  netAmount: number;
  vatAmount: number;
  totalAmount: number;
  currency: string;
  status: string;
  issuedAtUtc: string;
  pdfUrl: string;
};

export type CreateInvoiceRequest = {
  buyerName?: string;
  buyerEmail?: string;
  buyerTaxId?: string;
  billingAddress?: string;
};
