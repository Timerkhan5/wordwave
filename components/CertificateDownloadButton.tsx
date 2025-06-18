import React, { useState } from "react";
import { Modal, Button, Input, Form } from "antd";

const CertificateDownloadButton: React.FC = () => {
  const [visible, setVisible] = useState(false);
  const [loading, setLoading] = useState(false);
  const [form] = Form.useForm();

  const showModal = () => setVisible(true);

  const handleOk = async () => {
    try {
      const values = await form.validateFields();
      setLoading(true);
      setTimeout(() => {
        setLoading(false);
        setVisible(false);
        form.resetFields();
      }, 1000);
    } catch (err) {
    }
  };

  const handleCancel = () => {
    setVisible(false);
    form.resetFields();
  };

  return (
    <>
      <Button type="primary" onClick={showModal}>
        Скачать сертификат
      </Button>
      <Modal
        title="Скачать сертификат"
        visible={visible}
        onOk={handleOk}
        onCancel={handleCancel}
        confirmLoading={loading}
      >
        <Form form={form} layout="vertical">
          <Form.Item
            name="firstName"
            label="Имя"
            rules={[{ required: true, message: "Пожалуйста, введите имя" }]}
          >
            <Input />
          </Form.Item>
          <Form.Item
            name="lastName"
            label="Фамилия"
            rules={[{ required: true, message: "Пожалуйста, введите фамилию" }]}
          >
            <Input />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
};

export default CertificateDownloadButton;
