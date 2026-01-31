import { useState, useRef } from 'react';
import { extractInvoice } from './services/api';
import type { ExtractionResult } from './types/invoice';
import './App.css';

function App() {
  const [files, setFiles] = useState<File[]>([]);
  const [results, setResults] = useState<ExtractionResult[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      setFiles(Array.from(e.target.files));
      setError(null);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (files.length === 0) return;

    setLoading(true);
    setError(null);
    setResults([]);

    try {
      const response = await extractInvoice(files);
      setResults(response.results);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  const handleClear = () => {
    setFiles([]);
    setResults([]);
    setError(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  return (
    <div className="container">
      <h1>Invoice Extractor</h1>

      <form onSubmit={handleSubmit} className="upload-form">
        <div className="file-input-wrapper">
          <input
            ref={fileInputRef}
            type="file"
            accept="image/jpeg,image/png"
            multiple
            onChange={handleFileChange}
          />
        </div>

        {files.length > 0 && (
          <div className="selected-files">
            <p>Selected files:</p>
            <ul>
              {files.map((file, index) => (
                <li key={index}>{file.name}</li>
              ))}
            </ul>
          </div>
        )}

        <div className="button-group">
          <button type="submit" disabled={files.length === 0 || loading}>
            {loading ? 'Extracting...' : 'Extract Invoice'}
          </button>
          <button type="button" onClick={handleClear} disabled={loading}>
            Clear
          </button>
        </div>
      </form>

      {error && <div className="error">{error}</div>}

      {results.length > 0 && (
        <div className="results">
          <h2>Extraction Results</h2>
          {results.map((result, index) => (
            <div key={index} className="result-card">
              <h3>{result.fileName}</h3>
              <div className="invoice-info">
                <div className="info-row">
                  <span className="label">Invoice Number:</span>
                  <span className="value">{result.extractedInfo.invoiceNumber ?? '-'}</span>
                </div>
                <div className="info-row">
                  <span className="label">Vendor:</span>
                  <span className="value">{result.extractedInfo.vendorName ?? '-'}</span>
                </div>
                <div className="info-row">
                  <span className="label">Date:</span>
                  <span className="value">{result.extractedInfo.issuedDate ?? '-'}</span>
                </div>
                <div className="info-row">
                  <span className="label">Total Amount:</span>
                  <span className="value">
                    {result.extractedInfo.totalAmount != null
                      ? `$${result.extractedInfo.totalAmount.toFixed(2)}`
                      : '-'}
                  </span>
                </div>

                {result.extractedInfo.items.length > 0 && (
                  <div className="items-section">
                    <h4>Line Items</h4>
                    <table>
                      <thead>
                        <tr>
                          <th>Description</th>
                          <th>Qty</th>
                          <th>Unit</th>
                          <th>Unit Price</th>
                          <th>Amount</th>
                        </tr>
                      </thead>
                      <tbody>
                        {result.extractedInfo.items.map((item, itemIndex) => (
                          <tr key={itemIndex}>
                            <td>{item.description ?? '-'}</td>
                            <td>{item.quantity ?? '-'}</td>
                            <td>{item.unit ?? '-'}</td>
                            <td>{item.unitPrice != null ? `$${item.unitPrice.toFixed(2)}` : '-'}</td>
                            <td>{item.amount != null ? `$${item.amount.toFixed(2)}` : '-'}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default App;
