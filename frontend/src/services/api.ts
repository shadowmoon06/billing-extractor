import type { ExtractionResponse } from '../types/invoice';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:7001';

export async function extractInvoice(files: File[]): Promise<ExtractionResponse> {
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
