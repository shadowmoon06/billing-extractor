import { useState, useEffect } from 'react';
import { getInvoices, deleteInvoice } from '../services/api';
import type { InvoiceSummaryDto } from '../types/invoice';
import InvoiceDetailModal from '../components/InvoiceDetailModal';
import UploadInvoiceModal from '../components/UploadInvoiceModal';
import './InvoiceListPage.css';

function InvoiceListPage() {
  const [invoices, setInvoices] = useState<InvoiceSummaryDto[]>([]);
  const [filteredInvoices, setFilteredInvoices] = useState<InvoiceSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [selectedInvoice, setSelectedInvoice] = useState<string | null>(null);
  const [showUploadModal, setShowUploadModal] = useState(false);
  const itemsPerPage = 10;

  useEffect(() => {
    fetchInvoices();
  }, []);

  useEffect(() => {
    const filtered = invoices.filter(
      (invoice) =>
        invoice.invoiceNumber.toLowerCase().includes(searchQuery.toLowerCase()) ||
        invoice.vendorName.toLowerCase().includes(searchQuery.toLowerCase())
    );
    setFilteredInvoices(filtered);
    setCurrentPage(1);
  }, [searchQuery, invoices]);

  const fetchInvoices = async () => {
    try {
      setLoading(true);
      const data = await getInvoices();
      setInvoices(data);
      setFilteredInvoices(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch invoices');
    } finally {
      setLoading(false);
    }
  };

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

  const totalPages = Math.ceil(filteredInvoices.length / itemsPerPage);
  const startIndex = (currentPage - 1) * itemsPerPage;
  const paginatedInvoices = filteredInvoices.slice(startIndex, startIndex + itemsPerPage);

  const handlePrevPage = () => {
    if (currentPage > 1) setCurrentPage(currentPage - 1);
  };

  const handleNextPage = () => {
    if (currentPage < totalPages) setCurrentPage(currentPage + 1);
  };

  const handleViewDetail = (invoiceNumber: string) => {
    setSelectedInvoice(invoiceNumber);
  };

  const handleCloseDetail = () => {
    setSelectedInvoice(null);
  };

  const handleOpenUpload = () => {
    setShowUploadModal(true);
  };

  const handleCloseUpload = () => {
    setShowUploadModal(false);
  };

  const handleUploadSuccess = () => {
    fetchInvoices();
  };

  const handleDelete = async (invoiceNumber: string) => {
    if (!confirm(`Are you sure you want to delete invoice ${invoiceNumber}?`)) {
      return;
    }
    try {
      await deleteInvoice(invoiceNumber);
      fetchInvoices();
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to delete invoice');
    }
  };

  return (
    <div className="page-container">
      <div className="page-header">
        <div className="header-text">
          <h1>Invoices</h1>
          <p>Manage and view all your invoices</p>
        </div>
        <button className="btn-primary" onClick={handleOpenUpload}>
          Add Invoice
        </button>
      </div>

      <div className="card">
        <div className="table-header">
          <input
            type="text"
            className="search-input"
            placeholder="Search invoices..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
          <button className="btn-outline">Filter</button>
        </div>

        {loading ? (
          <div className="loading-state">Loading invoices...</div>
        ) : error ? (
          <div className="error-state">{error}</div>
        ) : (
          <>
            <div className="table-container">
              <table>
                <thead>
                  <tr>
                    <th className="col-invoice">Invoice #</th>
                    <th className="col-date">Issued Date</th>
                    <th className="col-vendor">Vendor Name</th>
                    <th className="col-amount">Total Amount</th>
                    <th className="col-edited">Last Edited</th>
                    <th className="col-actions" colSpan={2}>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {paginatedInvoices.length === 0 ? (
                    <tr>
                      <td colSpan={6} className="empty-state">
                        No invoices found
                      </td>
                    </tr>
                  ) : (
                    paginatedInvoices.map((invoice) => (
                      <tr key={invoice.invoiceNumber}>
                        <td className="col-invoice font-medium">{invoice.invoiceNumber}</td>
                        <td className="col-date text-muted">{formatDate(invoice.issuedDate)}</td>
                        <td className="col-vendor">{invoice.vendorName}</td>
                        <td className="col-amount font-medium">{formatCurrency(invoice.totalAmount)}</td>
                        <td className="col-edited text-muted">{formatDate(invoice.lastEdited)}</td>
                        <td className="col-actions">
                          <button
                            className="btn-ghost btn-small"
                            onClick={() => handleViewDetail(invoice.invoiceNumber)}
                          >
                            View
                          </button>
                        </td>
                        <td className="col-actions">
                          <button
                            className="btn-ghost btn-small text-red-600"
                            onClick={() => handleDelete(invoice.invoiceNumber)}
                          >
                            Delete
                          </button>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>

            <div className="table-footer">
              <span className="footer-text">
                Showing {paginatedInvoices.length} of {filteredInvoices.length} invoices
              </span>
              <div className="pagination">
                <button
                  className="btn-ghost"
                  onClick={handlePrevPage}
                  disabled={currentPage === 1}
                >
                  Previous
                </button>
                <div className="page-numbers">
                  {Array.from({ length: Math.min(totalPages, 3) }, (_, i) => i + 1).map((page) => (
                    <button
                      key={page}
                      className={`page-btn ${currentPage === page ? 'active' : ''}`}
                      onClick={() => setCurrentPage(page)}
                    >
                      {page}
                    </button>
                  ))}
                </div>
                <button
                  className="btn-ghost"
                  onClick={handleNextPage}
                  disabled={currentPage === totalPages || totalPages === 0}
                >
                  Next
                </button>
              </div>
            </div>
          </>
        )}
      </div>

      {/* Invoice Detail Modal */}
      {selectedInvoice && (
        <InvoiceDetailModal
          invoiceNumber={selectedInvoice}
          onClose={handleCloseDetail}
          onDelete={fetchInvoices}
        />
      )}

      {/* Upload Invoice Modal */}
      {showUploadModal && (
        <UploadInvoiceModal
          onClose={handleCloseUpload}
          onSuccess={handleUploadSuccess}
        />
      )}
    </div>
  );
}

export default InvoiceListPage;
