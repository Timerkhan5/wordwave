import React from "react";
import CertificateDownloadButton from "./components/CertificateDownloadButton";
import GenerateTasksButton from "./components/GenerateTasksButton";

const AdminPanel = () => {
  return (
    <div>
      {/* ...existing code... */}
      <CertificateDownloadButton />
      <GenerateTasksButton />
      {/* ...existing code... */}
    </div>
  );
};

export default AdminPanel;