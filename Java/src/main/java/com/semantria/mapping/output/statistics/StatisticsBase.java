package com.semantria.mapping.output.statistics;

import javax.xml.bind.annotation.XmlElement;

public abstract class StatisticsBase {
    private Long total_api_calls = null;
    private Long calls_settings = null;
    private Long calls_polling = null;
    private Long calls_data = null;
    private Long docs_queued = null;
    private Long docs_successful = null;
    private Long docs_failed = null;
    private Long docs_retrieved = null;
    private Long batches_queued = null;

    @XmlElement(name = "total_api_calls")
    public Long getTotalApiCalls() { return total_api_calls; }
    @XmlElement(name = "calls_settings")
    public Long getCallsSettings() { return calls_settings; }
    @XmlElement(name = "calls_polling")
    public Long getCallsPolling() { return calls_polling; }
    @XmlElement(name = "calls_data")
    public Long getCallsData() { return calls_data; }
    @XmlElement(name = "docs_queued")
    public Long getDocsQueued() { return docs_queued; }
    @XmlElement(name = "docs_successful")
    public Long getDocsSuccessful() { return docs_successful; }
    @XmlElement(name = "docs_failed")
    public Long getDocsFailed() { return docs_failed; }
    @XmlElement(name = "docs_retrieved")
    public Long getDocsRetrieved() { return docs_retrieved; }
    @XmlElement(name = "batches_queued")
    public Long getBatchesQueued() { return batches_queued; }

    public void setTotalApiCalls(Long total_api_calls) { this.total_api_calls = total_api_calls; }
    public void setCallsSettings(Long calls_settings) { this.calls_settings = calls_settings; }
    public void setCallsPolling(Long calls_polling) { this.calls_polling = calls_polling; }
    public void setCallsData(Long calls_data) { this.calls_data = calls_data; }
    public void setDocsQueued(Long docs_queued) { this.docs_queued = docs_queued; }
    public void setDocsSuccessful(Long docs_successful) { this.docs_successful = docs_successful; }
    public void setDocsFailed(Long docs_failed) { this.docs_failed = docs_failed; }
    public void setDocsRetrieved(Long docs_retrieved) { this.docs_retrieved = docs_retrieved; }
    public void setBatchesQueued(Long batches_queued) { this.batches_queued = batches_queued; }
}
