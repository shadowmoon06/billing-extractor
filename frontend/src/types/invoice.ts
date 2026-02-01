export interface InvoiceItemDto {
  itemId: string;
  description: string | null;
  quantity: number;
  unitPrice: number;
  unit: string | null;
  amount: number;
}

export interface InvoiceAdjustmentDto {
  description: string | null;
  amount: number;
}

export interface InvoiceSummaryDto {
  invoiceNumber: string;
  issuedDate: string;
  vendorName: string;
  totalAmount: number;
  lastEdited: string;
}

export interface InvoiceDetailDto {
  invoiceNumber: string;
  issuedDate: string;
  vendorName: string;
  totalAmount: number;
  lastEdited: string;
  items: InvoiceItemDto[];
  adjustments: InvoiceAdjustmentDto[];
}

export interface InvoiceItemInfo {
  itemId: string | null;
  description: string | null;
  quantity: number | null;
  unitPrice: number | null;
  unit: string | null;
  amount: number | null;
}

export interface InvoiceExtractedInfo {
  invoiceNumber: string | null;
  issuedDate: string | null;
  vendorName: string | null;
  totalAmount: number | null;
  items: InvoiceItemInfo[];
}

export interface ExtractionResult {
  fileName: string;
  contentType: string;
  size: number;
  extractedInfo: InvoiceExtractedInfo;
}

export interface ExtractionResponse {
  results: ExtractionResult[];
}
