return (
    <div>
      <h1>Редактирование экзамена</h1>
      <div style={{ maxHeight: '70vh', overflowY: 'auto', border: '1px solid #ccc', padding: 16, borderRadius: 8 }}>
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: 16 }}>
          {tasks.map(task => (
            <div
              key={task.id}
              style={{
                flex: '1 0 21%',
                minWidth: 220,
                maxWidth: '23%',
                boxSizing: 'border-box',
                marginBottom: 16,
                padding: 10,
                background: '#f9f9f9',
                borderRadius: 6,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
              }}
            >
              <span>{task.title}</span>
              <input
                type="checkbox"
                checked={task.isExam}
                onChange={() => toggleExam(task.id, task.isExam)}
                style={{ marginLeft: 12 }}
              />
            </div>
          ))}
        </div>
      </div>
    </div>
  );