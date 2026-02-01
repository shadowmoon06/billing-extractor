import { useState, useRef } from 'react';
import { extractInvoice, type ExtractInvoiceResponse } from '../services/api';
import './Modal.css';

interface UploadInvoiceModalProps {
  onClose: () => void;
  onSuccess: () => void;
}

const MAX_FILES = 10;

function UploadInvoiceModal({ onClose, onSuccess }: UploadInvoiceModalProps) {
  const [files, setFiles] = useState<File[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [result, setResult] = useState<ExtractInvoiceResponse | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      const selectedFiles = Array.from(e.target.files);
      if (selectedFiles.length > MAX_FILES) {
        setError(`Maximum ${MAX_FILES} images allowed per upload`);
        return;
      }
      setFiles(selectedFiles);
      setError(null);
      setResult(null);
    }
  };

  const handleDrop = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    const droppedFiles = Array.from(e.dataTransfer.files).filter(
      (file) => file.type === 'image/jpeg' || file.type === 'image/png'
    );
    if (droppedFiles.length > MAX_FILES) {
      setError(`Maximum ${MAX_FILES} images allowed per upload`);
      return;
    }
    if (droppedFiles.length > 0) {
      setFiles(droppedFiles);
      setError(null);
      setResult(null);
    }
  };

  const handleDragOver = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
  };

  const handleRemoveFile = (index: number) => {
    setFiles(files.filter((_, i) => i !== index));
  };

  const handleSubmit = async () => {
    if (files.length === 0) return;

    setLoading(true);
    setError(null);
    setResult(null);

    try {
      const response = await extractInvoice(files);
      setResult(response);
      if (response.savedInvoices.length > 0) {
        onSuccess();
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to process invoices');
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    if (!loading) {
      onClose();
    }
  };

  return (
    <div className="modal-overlay" onClick={handleClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Upload Invoice Images</h2>
          <button className="modal-close" onClick={handleClose} disabled={loading}>
            &times;
          </button>
        </div>

        <div className="modal-body">
          {!result ? (
            <>
              {/* Drop Zone */}
              <div
                className={`upload-dropzone ${loading ? 'disabled' : ''}`}
                onDrop={loading ? undefined : handleDrop}
                onDragOver={loading ? undefined : handleDragOver}
                onClick={loading ? undefined : () => fileInputRef.current?.click()}
              >
                <input
                  ref={fileInputRef}
                  type="file"
                  accept="image/jpeg,image/png"
                  multiple
                  onChange={handleFileChange}
                  style={{ display: 'none' }}
                />
                <div className="dropzone-content">
                  <span className="dropzone-icon">+</span>
                  <p>Drop images here or click to browse</p>
                  <span className="dropzone-hint">Supports JPG, PNG (max {MAX_FILES} files)</span>
                </div>
              </div>

              {/* File List */}
              {files.length > 0 && (
                <div className="file-list">
                  <h4>Selected Files ({files.length})</h4>
                  <ul>
                    {files.map((file, index) => (
                      <li key={index}>
                        <span className="file-name">{file.name}</span>
                        <span className="file-size">
                          {(file.size / 1024).toFixed(1)} KB
                        </span>
                        <button
                          className="file-remove"
                          onClick={() => handleRemoveFile(index)}
                          disabled={loading}
                        >
                          &times;
                        </button>
                      </li>
                    ))}
                  </ul>
                </div>
              )}

              {error && <div className="modal-error">{error}</div>}
            </>
          ) : (
            /* Result Display */
            <div className="upload-result">
              {result.savedInvoices.length > 0 && (
                <div className="result-success">
                  <h4>Successfully Saved ({result.savedInvoices.length})</h4>
                  <ul>
                    {result.savedInvoices.map((inv, index) => (
                      <li key={index}>
                        <strong>{inv.invoiceNumber}</strong> - {inv.vendorName}
                      </li>
                    ))}
                  </ul>
                </div>
              )}

              {result.duplicateInvoiceNumbers.length > 0 && (
                <div className="result-warning">
                  <h4>Duplicates Skipped ({result.duplicateInvoiceNumbers.length})</h4>
                  <ul>
                    {result.duplicateInvoiceNumbers.map((num, index) => (
                      <li key={index}>{num}</li>
                    ))}
                  </ul>
                </div>
              )}

              {result.savedInvoices.length === 0 && result.duplicateInvoiceNumbers.length === 0 && (
                <div className="result-info">
                  <p>No invoices were saved. Please check the uploaded images.</p>
                </div>
              )}
            </div>
          )}
        </div>

        <div className="modal-footer">
          {!result ? (
            <>
              <button className="btn-outline" onClick={handleClose} disabled={loading}>
                Cancel
              </button>
              <button
                className="btn-primary"
                onClick={handleSubmit}
                disabled={files.length === 0 || loading}
              >
                {loading ? 'Processing...' : 'Upload & Extract'}
              </button>
            </>
          ) : (
            <button className="btn-primary" onClick={handleClose}>
              Done
            </button>
          )}
        </div>
      </div>
    </div>
  );
}

export default UploadInvoiceModal;
