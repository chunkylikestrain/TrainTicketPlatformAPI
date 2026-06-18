import "./App.css";

function App() {
  return (
    <main className="page">
      <section className="hero">
        <h1>Train Ticket Platform</h1>
        <p>Search for trains and book your journey online.</p>

        <form className="search-form">
          <div className="form-field">
            <label htmlFor="from">From</label>
            <input
              id="from"
              type="text"
              placeholder="Rzeszów"
            />
          </div>

          <div className="form-field">
            <label htmlFor="to">To</label>
            <input
              id="to"
              type="text"
              placeholder="Kraków"
            />
          </div>

          <div className="form-field">
            <label htmlFor="date">Travel date</label>
            <input
              id="date"
              type="date"
            />
          </div>

          <button type="submit">Search trains</button>
        </form>
      </section>
    </main>
  );
}

export default App;
