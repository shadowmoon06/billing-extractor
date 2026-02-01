import { useState, useEffect } from 'react';
import { getInvoiceDetail, deleteInvoice } from '../services/api';
import type { InvoiceDetailDto } from '../types/invoice';
import './Modal.css';

interface InvoiceDetailModalProps {
  invoiceNumber: string;
  onClose: () => void;
  onDelete?: () => void;
}

function InvoiceDetailModal({ invoiceNumber, onClose, onDelete }: InvoiceDetailModalProps) {
  const [invoice, setInvoice] = useState<InvoiceDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [deleting, setDeleting] = useState(false);

  useEffect(() => {
    const fetchDetail = async () => {
      try {
        setLoading(true);
        const data = await getInvoiceDetail(invoiceNumber);
        setInvoice(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load invoice');
      } finally {
        setLoading(false);
      }
    };

    fetchDetail();
  }, [invoiceNumber]);

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      month: 'short',
      day: '2-digit',
      year: 'numeric',
    });
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(amount);
  };

  const handleDelete = async () => {
    if (!confirm(`Are you sure you want to delete invoice ${invoiceNumber}?`)) {
      return;
    }
    try {
      setDeleting(true);
      await deleteInvoice(invoiceNumber);
      onDelete?.();
      onClose();
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to delete invoice');
    } finally {
      setDeleting(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content modal-large" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Invoice Details</h2>
          <button className="modal-close" onClick={onClose}>
            &times;
          </button>
        </div>

        <div className="modal-body">
          {loading ? (
            <div className="modal-loading">Loading invoice details...</div>
          ) : error ? (
            <div className="modal-error">{error}</div>
          ) : invoice ? (
            <>
              {/* Summary Section */}
              <div className="detail-section">
                <div className="detail-grid detail-grid-3">
                  <div className="detail-item">
                    <span className="detail-label">Invoice Number</span>
                    <span className="detail-value font-medium">{invoice.invoiceNumber}</span>
                  </div>
                  <div className="detail-item">
                    <span className="detail-label">Vendor Name</span>
                    <span className="detail-value">{invoice.vendorName}</span>
                  </div>
                  <div className="detail-item">
                    <span className="detail-label">Issued Date</span>
                    <span className="detail-value">{formatDate(invoice.issuedDate)}</span>
                  </div>
                </div>
              </div>

              {/* Items Section */}
              <div className="detail-section">
                <h3>Line Items</h3>
                {invoice.items.length > 0 ? (
                  <div className="detail-table-container">
                    <table className="detail-table detail-table-items">
                      <thead>
                        <tr>
                          <th>Item ID</th>
                          <th className="col-description">Description</th>
                          <th>Qty</th>
                          <th>Unit</th>
                          <th>Unit Price</th>
                          <th className="col-amount">Amount</th>
                        </tr>
                      </thead>
                      <tbody>
                        {invoice.items.map((item, index) => (
                          <tr key={index}>
                            <td className="font-medium">{item.itemId}</td>
                            <td className="col-description">{item.description || '-'}</td>
                            <td>{item.quantity}</td>
                            <td>{item.unit || '-'}</td>
                            <td>{formatCurrency(item.unitPrice)}</td>
                            <td className="col-amount font-medium">{formatCurrency(item.amount)}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                ) : (
                  <p className="text-muted">No items found</p>
                )}
              </div>

              {/* Adjustments Section */}
              <div className="detail-section">
                <h3>Adjustments</h3>
                {invoice.adjustments.length > 0 ? (
                  <div className="detail-table-container">
                    <table className="detail-table detail-table-adjustments">
                      <thead>
                        <tr>
                          <th className="col-description">Description</th>
                          <th className="col-amount">Amount</th>
                        </tr>
                      </thead>
                      <tbody>
                        {invoice.adjustments.map((adj, index) => (
                          <tr key={index}>
                            <td className="col-description">{adj.description || '-'}</td>
                            <td className={`col-amount font-medium ${adj.amount < 0 ? 'text-destructive' : ''}`}>
                              {formatCurrency(adj.amount)}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                ) : (
                  <p className="text-muted">No adjustments</p>
                )}
              </div>

              {/* Total Amount */}
              <div className="detail-total">
                <span className="detail-total-label">Total Amount</span>
                <span className="detail-total-value">{formatCurrency(invoice.totalAmount)}</span>
              </div>
            </>
          ) : null}
        </div>

        {invoice && (
          <div className="modal-footer">
            <button
              className="btn-destructive"
              onClick={handleDelete}
              disabled={deleting}
            >
              {deleting ? 'Deleting...' : 'Delete Invoice'}
            </button>
          </div>
        )}
      </div>
    </div>
  );
}

export default InvoiceDetailModal;
