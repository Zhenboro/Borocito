// Text-Processing API Url
const API_URL = 'http://borocito/api.php';

const REQUEST_HEADERS = {
  'Content-Type': 'application/json',
  'Clase': 'COMMAND',
  'Ident': 'r9dbQJYc1JUrMwJM'
};

const userAction = async () => {
  let myBody=null;
  const response = await fetch(API_URL, {
    method: 'GET',
    headers: REQUEST_HEADERS
  });
  const myJson = await response.text(); //extract JSON from the http response
  // do something with myJson

  RenderTheThing(myJson);
}

function RenderTheThing(data) {
  // // Remove invisible class for main-result-block
  // const resultBlockElement = document.getElementById('main-result-block');
  // resultBlockElement.classList.remove('invisible');
  // Setting the color of the result text depending on the response label
  const resultElement = document.getElementById('result');
  let resultText = data;
  // Setting the result text
  resultElement.textContent = resultText;
}