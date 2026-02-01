import type { InvoiceSummaryDto, InvoiceDetailDto } from '../types/invoice';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:7001';

export interface ExtractInvoiceResponse {
  extractedCount: number;
  savedInvoices: Array<{
    invoiceNumber: string;
    vendorName: string;
    totalAmount: number;
  }>;
  skippedCount: number;
  duplicateInvoiceNumbers: string[];
}

export async function extractInvoice(files: File[]): Promise<ExtractInvoiceResponse> {
  const formData = new FormData();
  files.forEach((file) => {
    formData.append('images', file);
  });

  const response = await fetch(`${API_BASE_URL}/api/Invoice/extract`, {
    method: 'POST',
    body: formData,
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.errors?.join(', ') || 'Failed to extract invoice');
  }

  return response.json();
}

export async function getInvoices(): Promise<InvoiceSummaryDto[]> {
  const response = await fetch(`${API_BASE_URL}/api/Invoice`);

  if (!response.ok) {
    throw new Error('Failed to fetch invoices');
  }

  return response.json();
}

export async function getInvoiceDetail(invoiceNumber: string): Promise<InvoiceDetailDto> {
  const response = await fetch(`${API_BASE_URL}/api/Invoice/${encodeURIComponent(invoiceNumber)}`);

  if (!response.ok) {
    throw new Error('Failed to fetch invoice detail');
  }

  return response.json();
}

export async function deleteInvoice(invoiceNumber: string): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/api/Invoice/${encodeURIComponent(invoiceNumber)}`, {
    method: 'DELETE',
  });

  if (!response.ok) {
    throw new Error('Failed to delete invoice');
  }
}
